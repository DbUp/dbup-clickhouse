using ClickHouse.Driver.ADO;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DbUp.ClickHouse.Tests;

/// <summary>
/// Integration tests for ClickHouse database operations using TestContainers.
/// Tests both script execution and journal table operations against a real ClickHouse instance.
/// Uses class fixture to share container across all tests in the class.
/// </summary>
public class ClickHouseIntegrationTests : IClassFixture<ClickHouseContainerFixture>, IDisposable
{
    private readonly ClickHouseContainerFixture fixture;
    readonly ITestOutputHelper testOutputHelper;
    string ConnectionString => fixture.ConnectionString;

    public ClickHouseIntegrationTests(ClickHouseContainerFixture fixture, ITestOutputHelper testOutputHelper)
    {
        this.fixture = fixture;
        this.testOutputHelper = testOutputHelper;
    }

    public void Dispose()
    {
        // Clean up after each test to ensure test isolation
        try
        {
            using var connection = new ClickHouseConnection(ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            
            // Clear the journal table to ensure clean state for next test
            command.CommandText = "DROP TABLE IF EXISTS `testdb`.`schemaversions`";
            command.ExecuteNonQuery();
            
            // Also clean up any test tables that might have been created
            command.CommandText = "DROP TABLE IF EXISTS test_table_ddl";
            command.ExecuteNonQuery();
            
            command.CommandText = "DROP TABLE IF EXISTS insert_test_table";
            command.ExecuteNonQuery();
            
            command.CommandText = "DROP TABLE IF EXISTS multi_statement_test";
            command.ExecuteNonQuery();
            
            command.CommandText = "DROP TABLE IF EXISTS duplicate_test";
            command.ExecuteNonQuery();
            
            // Clean up custom database
            command.CommandText = "DROP DATABASE IF EXISTS custom_db";
            command.ExecuteNonQuery();
        }
        catch
        {
            // Ignore cleanup errors - they shouldn't fail the tests
        }
    }
    [Fact]
    public void ScriptExecution_DDL_CreateTable_ShouldExecuteSuccessfully()
    {
        // Arrange
        var upgrader = DeployChanges.To
            .ClickHouseDatabase(ConnectionString)
            .WithScript("001_CreateTestTable.sql", @"
                CREATE TABLE test_table_ddl 
                (
                    id UInt32,
                    name String,
                    created_at DateTime
                ) 
                ENGINE = MergeTree() 
                ORDER BY id")
            .LogToConsole()
            .Build();

        // Act
        var result = upgrader.PerformUpgrade();

        // Assert
        if (!result.Successful)
        {
            testOutputHelper.WriteLine($"[DEBUG_LOG] DbUp failed with error: {result.Error}");
            throw new Exception($"DbUp operation failed: {result.Error}");
        }
        result.Successful.ShouldBeTrue();
        result.Error.ShouldBeNull();
        var scriptsList = result.Scripts.ToList();
        scriptsList.Count.ShouldBe(1);
        scriptsList[0].Name.ShouldBe("001_CreateTestTable.sql");

        // Verify table was created
        using var connection = new ClickHouseConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "EXISTS TABLE test_table_ddl";
        var exists = command.ExecuteScalar();
        exists.ShouldBe(1);
    }

    [Fact]
    public void ScriptExecution_DML_InsertData_ShouldExecuteSuccessfully()
    {
        // Arrange - First create a table (not tracked in journal)
        using (var setupConnection = new ClickHouseConnection(ConnectionString))
        {
            setupConnection.Open();
            using var setupCommand = setupConnection.CreateCommand();
            setupCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS insert_test_table 
                (
                    id UInt32,
                    name String
                ) 
                ENGINE = MergeTree() 
                ORDER BY id";
            setupCommand.ExecuteNonQuery();
        }

        var upgrader = DeployChanges.To
            .ClickHouseDatabase(ConnectionString)
            .WithScript("001_InsertData.sql", @"
                INSERT INTO insert_test_table (id, name) VALUES 
                (1, 'Test Record 1'),
                (2, 'Test Record 2'),
                (3, 'Test Record 3')")
            .LogToConsole()
            .Build();

        // Act
        var result = upgrader.PerformUpgrade();

        // Assert
        result.Successful.ShouldBeTrue();
        result.Error.ShouldBeNull();
        var scriptsList = result.Scripts.ToList();
        scriptsList.Count.ShouldBe(1);

        // Verify data was inserted
        using var connection = new ClickHouseConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM insert_test_table";
        var count = command.ExecuteScalar();
        count.ShouldBe(3);
    }

    [Fact]
    public void ScriptExecution_MultipleStatements_ShouldExecuteSuccessfully()
    {
        // Arrange
        var upgrader = DeployChanges.To
            .ClickHouseDatabase(ConnectionString)
            .WithScript("001_MultipleStatements.sql", @"
                CREATE TABLE multi_statement_test1 
                (
                    id UInt32,
                    name String
                ) 
                ENGINE = MergeTree() 
                ORDER BY id;
                
                CREATE TABLE multi_statement_test2 
                (
                    id UInt32,
                    description String
                ) 
                ENGINE = MergeTree() 
                ORDER BY id;
                
                INSERT INTO multi_statement_test1 (id, name) VALUES (1, 'Test')")
            .LogToConsole()
            .Build();

        // Act
        var result = upgrader.PerformUpgrade();

        // Assert
        result.Successful.ShouldBeTrue();
        result.Error.ShouldBeNull();

        // Verify both tables were created and data was inserted
        using var connection = new ClickHouseConnection(ConnectionString);
        connection.Open();
        using var command1 = connection.CreateCommand();
        command1.CommandText = "EXISTS TABLE multi_statement_test1";
        command1.ExecuteScalar().ShouldBe(1);

        using var command2 = connection.CreateCommand();
        command2.CommandText = "EXISTS TABLE multi_statement_test2";
        command2.ExecuteScalar().ShouldBe(1);

        using var command3 = connection.CreateCommand();
        command3.CommandText = "SELECT COUNT(*) FROM multi_statement_test1";
        command3.ExecuteScalar().ShouldBe(1);
    }

    [Fact]
    public void ScriptExecution_InvalidSQL_ShouldFailGracefully()
    {
        // Arrange
        var upgrader = DeployChanges.To
            .ClickHouseDatabase(ConnectionString)
            .WithScript("001_InvalidSQL.sql", "INVALID SQL STATEMENT THAT SHOULD FAIL")
            .LogToConsole()
            .Build();

        // Act
        var result = upgrader.PerformUpgrade();

        // Assert
        result.Successful.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
    }

    [Fact]
    public void JournalTable_Creation_ShouldCreateCorrectSchema()
    {
        // Arrange & Act - The journal table should be created automatically
        var upgrader = DeployChanges.To
            .ClickHouseDatabase(ConnectionString)
            .WithScript("001_TestScript.sql", "SELECT 1")
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        // Assert
        result.ShouldSatisfyAllConditions(
            x => x.Successful.ShouldBeTrue(),
            x => x.Error.ShouldBeNull(),
            x => x.Scripts.Count().ShouldBe(1),
            x => x.Scripts.First().Name.ShouldBe("001_TestScript.sql")
            );

        // Verify the journal table exists and has the correct structure
        using var connection = new ClickHouseConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        
        // Check if the journal table exists and get its structure
        command.CommandText = "DESCRIBE TABLE `testdb`.`schemaversions`";
        using var reader = command.ExecuteReader();
        
        var columns = new List<(string Name, string Type)>();
        while (reader.Read())
        {
            columns.Add((reader["name"].ToString() ?? "", reader["type"].ToString() ?? ""));
        }

        // Verify the schema
        columns.Count.ShouldBe(2);
        columns.ShouldContain(c => c.Name == "ScriptName" && c.Type == "String");
        columns.ShouldContain(c => c.Name == "Applied" && c.Type == "DateTime");
    }

    [Fact]
    public void JournalTable_StoreExecutedScript_ShouldRecordExecution()
    {
        // Arrange & Act
        var upgrader = DeployChanges.To
            .ClickHouseDatabase(ConnectionString)
            .WithScript("001_FirstScript.sql", "SELECT 1 as test_column")
            .WithScript("002_SecondScript.sql", "SELECT 2 as test_column")
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        // Assert
        result.Successful.ShouldBeTrue();

        // Verify scripts were recorded in journal
        using var connection = new ClickHouseConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        
        command.CommandText = "SELECT ScriptName FROM `testdb`.`schemaversions` ORDER BY ScriptName";
        using var reader = command.ExecuteReader();
        
        var scripts = new List<string>();
        while (reader.Read())
        {
            scripts.Add(reader["ScriptName"].ToString() ?? "");
        }

        scripts.Count.ShouldBe(2);
        scripts.ShouldContain("001_FirstScript.sql");
        scripts.ShouldContain("002_SecondScript.sql");
    }

    [Fact]
    public void JournalTable_RetrieveExecutedScripts_ShouldReturnCorrectList()
    {
        // Arrange - Execute some scripts first
        var firstUpgrader = DeployChanges.To
            .ClickHouseDatabase(ConnectionString)
            .WithScript("001_InitialScript.sql", "SELECT 1")
            .WithScript("002_SecondScript.sql", "SELECT 2")
            .LogToConsole()
            .Build();

        firstUpgrader.PerformUpgrade();

        // Act - Try to run upgrader again with additional scripts
        var secondUpgrader = DeployChanges.To
            .ClickHouseDatabase(ConnectionString)
            .WithScript("001_InitialScript.sql", "SELECT 1") // Should be skipped
            .WithScript("002_SecondScript.sql", "SELECT 2") // Should be skipped
            .WithScript("003_NewScript.sql", "SELECT 3") // Should be executed
            .LogToConsole()
            .Build();

        var result = secondUpgrader.PerformUpgrade();

        // Assert
        result.Successful.ShouldBeTrue();
        // Only the new script should have been executed
        var scriptsList = result.Scripts.ToList();
        scriptsList.Count.ShouldBe(1);
        scriptsList[0].Name.ShouldBe("003_NewScript.sql");
    }

    [Fact]
    public void JournalTable_DuplicateScriptDetection_ShouldSkipAlreadyExecuted()
    {
        // Arrange - Execute a script
        var firstUpgrader = DeployChanges.To
            .ClickHouseDatabase(ConnectionString)
            .WithScript("001_TestScript.sql", "CREATE TABLE duplicate_test (id UInt32) ENGINE = MergeTree() ORDER BY id")
            .LogToConsole()
            .Build();

        var firstResult = firstUpgrader.PerformUpgrade();
        firstResult.Successful.ShouldBeTrue();

        // Act - Try to run the same script again
        var secondUpgrader = DeployChanges.To
            .ClickHouseDatabase(ConnectionString)
            .WithScript("001_TestScript.sql", "CREATE TABLE duplicate_test (id UInt32) ENGINE = MergeTree() ORDER BY id")
            .LogToConsole()
            .Build();

        var secondResult = secondUpgrader.PerformUpgrade();

        // Assert
        secondResult.Successful.ShouldBeTrue();
        // No scripts should have been executed the second time
        var scriptsList = secondResult.Scripts.ToList();
        scriptsList.Count.ShouldBe(0);
    }

    [Fact]
    public void JournalTable_CustomDatabase_ShouldWorkCorrectly()
    {
        // Arrange - Use a custom database
        var customConnectionString = ConnectionString.Replace("Database=testdb", "Database=custom_db");
        
        // First create the custom database
        using (var connection = new ClickHouseConnection(ConnectionString))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "CREATE DATABASE IF NOT EXISTS custom_db";
            command.ExecuteNonQuery();
        }

        var upgrader = DeployChanges.To
            .ClickHouseDatabase(customConnectionString)
            .WithScript("001_CustomDatabaseTest.sql", "SELECT 1")
            .LogToConsole()
            .Build();

        // Act
        var result = upgrader.PerformUpgrade();

        // Assert
        result.Successful.ShouldBeTrue();

        // Verify journal table was created in a custom database
        using var customConnection = new ClickHouseConnection(customConnectionString);
        customConnection.Open();
        using var verifyCommand = customConnection.CreateCommand();
        verifyCommand.CommandText = "SELECT ScriptName FROM `custom_db`.`schemaversions`";
        using var reader = verifyCommand.ExecuteReader();
        
        reader.Read().ShouldBeTrue();
        (reader["ScriptName"].ToString() ?? "").ShouldBe("001_CustomDatabaseTest.sql");
    }

    [Fact]
    public void JournalTable_CustomSchema_ShouldCreateInSpecifiedSchema()
    {
        // Arrange - Use a custom schema within the default database
        const string customSchema = "custom_schema";
        
        // First, create the custom schema (database in ClickHouse)
        using (var setupConnection = new ClickHouseConnection(ConnectionString))
        {
            setupConnection.Open();
            using var setupCommand = setupConnection.CreateCommand();
            setupCommand.CommandText = $"CREATE DATABASE IF NOT EXISTS {customSchema}";
            setupCommand.ExecuteNonQuery();
        }

        var upgrader = DeployChanges.To
            .ClickHouseDatabase(ConnectionString, customSchema)
            .WithScript("001_CustomSchemaTest.sql", "SELECT 1")
            .LogToConsole()
            .Build();

        // Act
        var result = upgrader.PerformUpgrade();

        // Assert
        result.Successful.ShouldBeTrue();

        // Verify journal table was created in the custom schema
        using var verifyConnection = new ClickHouseConnection(ConnectionString);
        verifyConnection.Open();
        using var verifyCommand = verifyConnection.CreateCommand();
        verifyCommand.CommandText = $"SELECT ScriptName FROM `{customSchema}`.`schemaversions`";
        using var reader = verifyCommand.ExecuteReader();
        
        reader.Read().ShouldBeTrue();
        (reader["ScriptName"].ToString() ?? "").ShouldBe("001_CustomSchemaTest.sql");
        
        // Verify the table exists in the correct schema
        using var schemaCheckCommand = verifyConnection.CreateCommand();
        schemaCheckCommand.CommandText = $"SELECT 1 FROM information_schema.tables WHERE table_name = 'schemaversions' AND table_schema = '{customSchema}'";
        var tableExists = schemaCheckCommand.ExecuteScalar();
        tableExists.ShouldNotBeNull();
    }
}

using System.Data;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Tests.Common;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DbUp.ClickHouse.Tests;

public class ClickHouseJournalTests
{
    // Test helper class to expose protected methods
    private class TestableClickHouseJournal : ClickHouseJournal
    {
        public TestableClickHouseJournal(
            Func<IConnectionManager> connectionManager,
            Func<IUpgradeLog> logger,
            string schema,
            string tableName)
            : base(connectionManager, logger, schema, tableName)
        {
        }

        public string TestGetJournalEntriesSql() => GetJournalEntriesSql();
        public string TestGetInsertJournalEntrySql(string scriptName, string applied) => GetInsertJournalEntrySql(scriptName, applied);
    }

    [Fact]
    public void StoreExecutedScript_CreatesCorrectInsertStatement()
    {
        // Arrange
        var dbConnection = Substitute.For<IDbConnection>();
        var connectionManager = new TestConnectionManager(dbConnection);
        var command = Substitute.For<IDbCommand>();
        var param1 = Substitute.For<IDbDataParameter>();
        var param2 = Substitute.For<IDbDataParameter>();
        dbConnection.CreateCommand().Returns(command);
        command.CreateParameter().Returns(param1, param2);
        command.ExecuteScalar().Returns(x => 0);
        var consoleUpgradeLog = new ConsoleUpgradeLog();
        var journal = new ClickHouseJournal(() => connectionManager, () => consoleUpgradeLog, "default",
            "SchemaVersions");

        // Act
        journal.StoreExecutedScript(new SqlScript("test", "select 1"), () => command);

        // Assert
        command.Received(2).CreateParameter();
        command.CommandText.ShouldBe("INSERT INTO `default`.`SchemaVersions` (ScriptName, Applied) VALUES (@scriptName, @applied)");
        command.Received().ExecuteNonQuery();
    }

    [Fact]
    public void GetJournalEntriesSql_GeneratesCorrectSelectStatement()
    {
        // Arrange
        var dbConnection = Substitute.For<IDbConnection>();
        var connectionManager = new TestConnectionManager(dbConnection);
        var consoleUpgradeLog = new ConsoleUpgradeLog();
        var journal = new TestableClickHouseJournal(() => connectionManager, () => consoleUpgradeLog, "default",
            "SchemaVersions");

        // Act
        var sql = journal.TestGetJournalEntriesSql();

        // Assert
        sql.ShouldBe("SELECT ScriptName FROM `default`.`SchemaVersions` ORDER BY ScriptName");
    }

    [Fact]
    public void GetInsertJournalEntrySql_GeneratesCorrectInsertStatement()
    {
        // Arrange
        var dbConnection = Substitute.For<IDbConnection>();
        var connectionManager = new TestConnectionManager(dbConnection);
        var consoleUpgradeLog = new ConsoleUpgradeLog();
        var journal = new TestableClickHouseJournal(() => connectionManager, () => consoleUpgradeLog, "default",
            "SchemaVersions");

        // Act
        var sql = journal.TestGetInsertJournalEntrySql("@scriptName", "@applied");

        // Assert
        sql.ShouldBe("INSERT INTO `default`.`SchemaVersions` (ScriptName, Applied) VALUES (@scriptName, @applied)");
    }

    [Fact]
    public void EnsureTableExists_CreatesCorrectTableCreationSql()
    {
        // Arrange
        var dbConnection = Substitute.For<IDbConnection>();
        var connectionManager = new TestConnectionManager(dbConnection);
        var command = Substitute.For<IDbCommand>();
        dbConnection.CreateCommand().Returns(command);
        command.ExecuteScalar().Returns(0); // Table doesn't exist
        var consoleUpgradeLog = new ConsoleUpgradeLog();
        var journal = new ClickHouseJournal(() => connectionManager, () => consoleUpgradeLog, "default",
            "SchemaVersions");

        // Act
        journal.EnsureTableExistsAndIsLatestVersion(() => command);

        // Assert
        // Verify the create table command was executed
        command.Received().ExecuteNonQuery();
        // The exact SQL will be set when the table doesn't exist
        command.CommandText.ShouldNotBeNullOrEmpty();
    }
}

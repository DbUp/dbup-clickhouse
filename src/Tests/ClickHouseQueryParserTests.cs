using Xunit;

namespace DbUp.ClickHouse.Tests;

public class ClickHouseQueryParserTests
{
    [Theory]
    [InlineData("SELECT 1\n;\nSELECT 2", 2, "SELECT 1", "SELECT 2")]
    [InlineData(";;SELECT 1", 1, "SELECT 1")]
    [InlineData("SELECT 1;", 1, "SELECT 1")]
    [InlineData("", 0)]
    // ClickHouse parser now properly handles comments - doesn't split on semicolon inside comments
    [InlineData("SELECT 1 /* block comment; */", 1, "SELECT 1 /* block comment; */")]
    [InlineData(
        """
        SELECT 1;
        -- Line comment; with semicolon
        SELECT 2;
        """, 2,
        "SELECT 1",
        """
        -- Line comment; with semicolon
        SELECT 2
        """)]
    // ClickHouse parser now properly handles string literals - doesn't split on semicolon inside strings
    [InlineData("SELECT 'string with; semicolon'", 1, "SELECT 'string with; semicolon'")]
    // ClickHouse parser now properly handles quoted identifiers - doesn't split on semicolon inside quotes
    [InlineData("SELECT 1 as `QUOTED;IDENT`", 1, "SELECT 1 as `QUOTED;IDENT`")]
    [InlineData("""
                CREATE TABLE test (
                    id UInt32,
                    name String
                ) ENGINE = MergeTree()
                ORDER BY id;
                INSERT INTO test VALUES (1, 'test');
                """, 2)]
    [InlineData("""
                CREATE VIEW test_view AS
                SELECT * FROM test;
                SELECT COUNT(*) FROM test_view;
                """, 2)]
    public void split_into_statements(string sql, int statementCount, params string[] expected)
    {
        var results = ParseCommand(sql);
        Assert.Equal(statementCount, results.Count);
        if (expected.Length > 0)
            Assert.Equal(expected, results);
    }

    [Fact]
    public void split_single_statement_no_semicolon()
    {
        const string sql = "SELECT 1";
        var results = ParseCommand(sql);
        Assert.Single(results);
        Assert.Equal(sql, results[0]);
    }

    [Fact]
    public void split_handles_empty_statements()
    {
        const string sql = ";;; SELECT 1 ;;;";
        var results = ParseCommand(sql);
        Assert.Single(results);
        Assert.Equal("SELECT 1", results[0]);
    }

    [Fact]
    public void split_handles_multiline_statements()
    {
        const string sql = """
            CREATE TABLE test (
                id UInt32,
                name String
            ) ENGINE = MergeTree()
            ORDER BY id;
            
            INSERT INTO test 
            VALUES (1, 'test')
            """;
        var results = ParseCommand(sql);
        Assert.Equal(2, results.Count);
        Assert.Contains("CREATE TABLE test", results[0]);
        Assert.Contains("INSERT INTO test", results[1]);
    }

    private static List<string> ParseCommand(string sql)
    {
        var manager = new ClickHouseConnectionManager("");
        var commands = manager.SplitScriptIntoCommands(sql);
        return commands.ToList();
    }
}

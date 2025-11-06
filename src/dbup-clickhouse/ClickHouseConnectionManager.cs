using System.Collections.Generic;
using System.Data;
using System.Linq;
using ClickHouse.Driver.ADO;
using DbUp.Engine.Transactions;

namespace DbUp.ClickHouse;

/// <summary>
/// Manages ClickHouse database connections.
/// </summary>
public class ClickHouseConnectionManager : DatabaseConnectionManager
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClickHouseConnectionManager"/> class.
    /// </summary>
    /// <param name="connectionString">The ClickHouse connection string.</param>
    public ClickHouseConnectionManager(string connectionString)
        : base(new DelegateConnectionFactory(_ => CreateConnection(connectionString)))
    {
    }

    private static IDbConnection CreateConnection(string connectionString)
    {
        return new ClickHouseConnection(connectionString);
    }

    /// <summary>
    /// Splits the statements in the script using proper SQL parsing that handles comments and string literals.
    /// </summary>
    /// <param name="scriptContents">The contents of the script to split.</param>
    /// <returns>A collection of individual SQL statements.</returns>
    public override IEnumerable<string> SplitScriptIntoCommands(string scriptContents)
    {
        var statements = ClickHouseQueryParser.ParseRawQuery(scriptContents);
        return statements
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToArray();
    }
}


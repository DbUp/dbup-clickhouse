using DbUp.Builder;
using DbUp.Engine.Transactions;

namespace DbUp.ClickHouse;

/// <summary>
/// Configuration extension methods for ClickHouse.
/// </summary>
public static class ClickHouseExtensions
{
    public static UpgradeEngineBuilder ClickHouseDatabase(this SupportedDatabases supported, string connectionString)
        => ClickHouseDatabase(supported, connectionString, null);

    public static UpgradeEngineBuilder ClickHouseDatabase(this SupportedDatabases supported, string connectionString, string database)
        => ClickHouseDatabase(new ClickHouseConnectionManager(connectionString), database);

    public static UpgradeEngineBuilder ClickHouseDatabase(this SupportedDatabases supported, IConnectionManager connectionManager)
        => ClickHouseDatabase(connectionManager);

    public static UpgradeEngineBuilder ClickHouseDatabase(IConnectionManager connectionManager)
        => ClickHouseDatabase(connectionManager, null);

    public static UpgradeEngineBuilder ClickHouseDatabase(IConnectionManager connectionManager, string schema)
    {
        var builder = new UpgradeEngineBuilder();
        builder.Configure(c => c.ConnectionManager = connectionManager);
        builder.Configure(c => c.ScriptExecutor = new ClickHouseScriptExecutor(() => c.ConnectionManager, () => c.Log, schema, () => c.VariablesEnabled, c.ScriptPreprocessors, () => c.Journal));
        builder.Configure(c => c.Journal = new ClickHouseJournal(() => c.ConnectionManager, () => c.Log, schema, "schemaversions"));
        builder.WithPreprocessor(new ClickHousePreprocessor());
        return builder;
    }

    public static UpgradeEngineBuilder JournalToClickHouseTable(this UpgradeEngineBuilder builder, string schema, string table)
    {
        builder.Configure(c => c.Journal = new ClickHouseJournal(() => c.ConnectionManager, () => c.Log, schema, table));
        return builder;
    }
}


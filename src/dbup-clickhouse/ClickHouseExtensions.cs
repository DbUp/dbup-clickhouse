using DbUp.Builder;
using DbUp.Engine.Transactions;

namespace DbUp.ClickHouse;

/// <summary>
/// Configuration extension methods for ClickHouse.
/// </summary>
public static class ClickHouseExtensions
{
    /// <summary>
    /// Creates an upgrader for ClickHouse databases.
    /// </summary>
    /// <param name="supported">Fluent helper type.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>
    /// A builder for a database upgrader designed for ClickHouse databases.
    /// </returns>
    public static UpgradeEngineBuilder ClickHouseDatabase(this SupportedDatabases supported, string connectionString)
        => ClickHouseDatabase(supported, connectionString, null);

    /// <summary>
    /// Creates an upgrader for ClickHouse databases.
    /// </summary>
    /// <param name="supported">Fluent helper type.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="database">The ClickHouse database name to use.</param>
    /// <returns>
    /// A builder for a database upgrader designed for ClickHouse databases.
    /// </returns>
    public static UpgradeEngineBuilder ClickHouseDatabase(this SupportedDatabases supported, string connectionString, string database)
        => ClickHouseDatabase(new ClickHouseConnectionManager(connectionString), database);

    /// <summary>
    /// Creates an upgrader for ClickHouse databases.
    /// </summary>
    /// <param name="supported">Fluent helper type.</param>
    /// <param name="connectionManager">The <see cref="IConnectionManager"/> to be used during a database
    /// upgrade. See <see cref="ClickHouseConnectionManager"/> for an example implementation</param>
    /// <returns>
    /// A builder for a database upgrader designed for ClickHouse databases.
    /// </returns>
    public static UpgradeEngineBuilder ClickHouseDatabase(this SupportedDatabases supported, IConnectionManager connectionManager)
        => ClickHouseDatabase(connectionManager);

    /// <summary>
    /// Creates an upgrader for ClickHouse databases.
    /// </summary>
    /// <param name="connectionManager">The <see cref="IConnectionManager"/> to be used during a database
    /// upgrade. See <see cref="ClickHouseConnectionManager"/> for an example implementation</param>
    /// <returns>
    /// A builder for a database upgrader designed for ClickHouse databases.
    /// </returns>
    public static UpgradeEngineBuilder ClickHouseDatabase(IConnectionManager connectionManager)
        => ClickHouseDatabase(connectionManager, null);

    /// <summary>
    /// Creates an upgrader for ClickHouse databases.
    /// </summary>
    /// <param name="connectionManager">The <see cref="IConnectionManager"/> to be used during a database
    /// upgrade. See <see cref="ClickHouseConnectionManager"/> for an example implementation</param>
    /// <param name="schema">The database name to use.</param>
    /// <returns>
    /// A builder for a database upgrader designed for ClickHouse databases.
    /// </returns>
    public static UpgradeEngineBuilder ClickHouseDatabase(IConnectionManager connectionManager, string schema)
    {
        var builder = new UpgradeEngineBuilder();
        builder.Configure(c => c.ConnectionManager = connectionManager);
        builder.Configure(c => c.ScriptExecutor = new ClickHouseScriptExecutor(() => c.ConnectionManager, () => c.Log, schema, () => c.VariablesEnabled, c.ScriptPreprocessors, () => c.Journal));
        builder.Configure(c => c.Journal = new ClickHouseJournal(() => c.ConnectionManager, () => c.Log, schema, "schemaversions"));
        builder.WithPreprocessor(new ClickHousePreprocessor());
        return builder;
    }

    /// <summary>
    /// Tracks the list of executed scripts in a ClickHouse table.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="schema">The database.</param>
    /// <param name="table">The table.</param>
    /// <returns></returns>
    public static UpgradeEngineBuilder JournalToClickHouseTable(this UpgradeEngineBuilder builder, string schema, string table)
    {
        builder.Configure(c => c.Journal = new ClickHouseJournal(() => c.ConnectionManager, () => c.Log, schema, table));
        return builder;
    }
}


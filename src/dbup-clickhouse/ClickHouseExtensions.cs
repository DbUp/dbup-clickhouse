using System;
using DbUp.Builder;

namespace DbUp.ClickHouse;

/// <summary>
/// Configuration extension methods for ClickHouse.
/// </summary>
public static class ClickHouseExtensions
{
    /// <summary>
    /// Creates an upgrader for ClickHouse databases.
    /// </summary>
    /// <param name="supportedDatabases">Fluent helper type.</param>
    /// <param name="connectionString">ClickHouse database connection string.</param>
    /// <returns>
    /// A builder for a database upgrader designed for ClickHouse databases.
    /// </returns>
    public static UpgradeEngineBuilder ClickHouseDatabase(this SupportedDatabases supportedDatabases, string connectionString)
    {
        throw new NotImplementedException();
    }
}

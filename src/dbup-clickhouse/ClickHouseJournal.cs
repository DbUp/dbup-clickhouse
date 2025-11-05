using System;
using System.Data;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;

namespace DbUp.ClickHouse;

/// <summary>
/// An implementation of the <see cref="IJournal"/> interface for ClickHouse databases.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ClickHouseJournal"/> class.
/// </remarks>
/// <param name="connectionManagerFactory">The connection manager factory.</param>
/// <param name="logFactory">The log factory.</param>
/// <param name="tableName">The name of the table.</param>
public class ClickHouseJournal(
// Remove pragma once implemented
#pragma warning disable CS9113 // Parameter is unread.
    Func<IConnectionManager> connectionManagerFactory,
    Func<IUpgradeLog> logFactory,
    string tableName
#pragma warning restore CS9113 // Parameter is unread.
    ) : IJournal
{
    /// <inheritdoc/>
    public string[] GetExecutedScripts() => throw new NotImplementedException();

    /// <inheritdoc/>
    public void StoreExecutedScript(SqlScript script, Func<IDbCommand> dbCommandFactory) => throw new NotImplementedException();

    /// <inheritdoc/>
    public void EnsureTableExistsAndIsLatestVersion(Func<IDbCommand> dbCommandFactory) => throw new NotImplementedException();
}

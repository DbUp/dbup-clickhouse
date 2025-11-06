using System;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Support;

namespace DbUp.ClickHouse;

/// <summary>
/// An implementation of <see cref="ScriptExecutor"/> that executes against a ClickHouse database.
/// </summary>
public class ClickHouseScriptExecutor : ScriptExecutor
{
    /// <summary>
    /// Initializes an instance of the <see cref="ClickHouseScriptExecutor"/> class.
    /// </summary>
    public ClickHouseScriptExecutor(
        Func<IConnectionManager> connectionManagerFactory,
        Func<IUpgradeLog> log,
        string schema,
        Func<bool> variablesEnabled,
        System.Collections.Generic.IEnumerable<IScriptPreprocessor> scriptPreprocessors,
        Func<IJournal> journalFactory)
        : base(connectionManagerFactory, new ClickHouseObjectParser(), log, schema, variablesEnabled, scriptPreprocessors, journalFactory)
    {
    }

    protected override string GetVerifySchemaSql(string schema)
        => $"CREATE DATABASE IF NOT EXISTS {schema}";

    protected override void ExecuteCommandsWithinExceptionHandler(int index, SqlScript script, Action executeCommand)
    {
        try
        {
            executeCommand();
        }
        catch (Exception exception)
        {
            Log().LogInformation("ClickHouse exception has occurred in script: '{0}'", script.Name);
            Log().LogError("Script block number: {0}; Message: {1}", index, exception.Message);
            Log().LogError(exception.ToString());
            throw;
        }
    }
}


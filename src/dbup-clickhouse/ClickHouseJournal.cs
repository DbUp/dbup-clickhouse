using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Support;

namespace DbUp.ClickHouse;

/// <summary>
/// An implementation of the <see cref="Engine.IJournal"/> interface which tracks version numbers for a
/// ClickHouse database using a table called schemaversions.
/// </summary>
public class ClickHouseJournal : TableJournal
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClickHouseJournal"/> class.
    /// </summary>
    /// <param name="connectionManager">The connection manager.</param>
    /// <param name="logger">The log.</param>
    /// <param name="schema">The database that contains the table.</param>
    /// <param name="tableName">The table name.</param>
    public ClickHouseJournal(
        System.Func<IConnectionManager> connectionManager,
        System.Func<IUpgradeLog> logger,
        string schema,
        string tableName)
        : base(connectionManager, logger, new ClickHouseObjectParser(), schema, tableName)
    {
    }

    /// <inheritdoc/>
    protected override string GetInsertJournalEntrySql(string scriptName, string applied)
        => $"INSERT INTO {FqSchemaTableName} (ScriptName, Applied) VALUES ({scriptName}, {applied})";

    /// <inheritdoc/>
    protected override string GetJournalEntriesSql()
        => $"SELECT ScriptName FROM {FqSchemaTableName} ORDER BY ScriptName";

    /// <inheritdoc/>
    protected override string CreateSchemaTableSql(string quotedPrimaryKeyName)
        => $"""
            CREATE TABLE {FqSchemaTableName}
            (
                ScriptName String,
                Applied DateTime
            )
            ENGINE = MergeTree()
            ORDER BY (ScriptName)
            """;
    
    /// <inheritdoc/>
    protected override string DoesTableExistSql()
    {
        return string.IsNullOrEmpty(SchemaTableSchema)
            ? $"EXISTS TABLE {UnquotedSchemaTableName}"
            : $"EXISTS TABLE `{SchemaTableSchema}`.`{UnquotedSchemaTableName}`";
    }
}


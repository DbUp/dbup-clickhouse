using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Support;

namespace DbUp.ClickHouse;

/// <summary>
/// Tracks the list of executed scripts in a ClickHouse table.
/// </summary>
public class ClickHouseJournal : TableJournal
{
    public ClickHouseJournal(
        System.Func<IConnectionManager> connectionManager,
        System.Func<IUpgradeLog> logger,
        string schema,
        string tableName)
        : base(connectionManager, logger, new ClickHouseObjectParser(), schema, tableName)
    {
    }

    protected override string GetInsertJournalEntrySql(string scriptName, string applied)
        => $"INSERT INTO {FqSchemaTableName} (ScriptName, Applied) VALUES ({scriptName}, {applied})";

    protected override string GetJournalEntriesSql()
        => $"SELECT ScriptName FROM {FqSchemaTableName} ORDER BY ScriptName";

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
    protected override string DoesTableExistSql()
    {
        return string.IsNullOrEmpty(SchemaTableSchema)
            ? $"EXISTS TABLE {UnquotedSchemaTableName}"
            : $"EXISTS TABLE `{SchemaTableSchema}`.`{UnquotedSchemaTableName}`";
    }
}


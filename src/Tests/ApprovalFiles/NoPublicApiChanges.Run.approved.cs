[assembly: System.Reflection.AssemblyMetadata("RepositoryUrl", "https://github.com/DbUp/dbup-clickhouse.git")]
[assembly: System.Runtime.Versioning.TargetFramework(".NETCoreApp,Version=v8.0", FrameworkDisplayName=".NET 8.0")]
namespace DbUp.ClickHouse
{
    public class ClickHouseConnectionManager : DbUp.Engine.Transactions.DatabaseConnectionManager
    {
        public ClickHouseConnectionManager(string connectionString) { }
        public override System.Collections.Generic.IEnumerable<string> SplitScriptIntoCommands(string scriptContents) { }
    }
    public static class ClickHouseExtensions
    {
        public static DbUp.Builder.UpgradeEngineBuilder ClickHouseDatabase(DbUp.Engine.Transactions.IConnectionManager connectionManager) { }
        public static DbUp.Builder.UpgradeEngineBuilder ClickHouseDatabase(this DbUp.Builder.SupportedDatabases supported, DbUp.Engine.Transactions.IConnectionManager connectionManager) { }
        public static DbUp.Builder.UpgradeEngineBuilder ClickHouseDatabase(this DbUp.Builder.SupportedDatabases supported, string connectionString) { }
        public static DbUp.Builder.UpgradeEngineBuilder ClickHouseDatabase(DbUp.Engine.Transactions.IConnectionManager connectionManager, string schema) { }
        public static DbUp.Builder.UpgradeEngineBuilder ClickHouseDatabase(this DbUp.Builder.SupportedDatabases supported, string connectionString, string database) { }
        public static DbUp.Builder.UpgradeEngineBuilder JournalToClickHouseTable(this DbUp.Builder.UpgradeEngineBuilder builder, string schema, string table) { }
    }
    public class ClickHouseJournal : DbUp.Support.TableJournal
    {
        public ClickHouseJournal(System.Func<DbUp.Engine.Transactions.IConnectionManager> connectionManager, System.Func<DbUp.Engine.Output.IUpgradeLog> logger, string schema, string tableName) { }
        protected override string CreateSchemaTableSql(string quotedPrimaryKeyName) { }
        protected override string DoesTableExistSql() { }
        protected override string GetInsertJournalEntrySql(string scriptName, string applied) { }
        protected override string GetJournalEntriesSql() { }
    }
    public class ClickHouseObjectParser : DbUp.Support.SqlObjectParser
    {
        public ClickHouseObjectParser() { }
    }
    public class ClickHousePreprocessor : DbUp.Engine.IScriptPreprocessor
    {
        public ClickHousePreprocessor() { }
        public string Process(string contents) { }
    }
    public class ClickHouseScriptExecutor : DbUp.Support.ScriptExecutor
    {
        public ClickHouseScriptExecutor(System.Func<DbUp.Engine.Transactions.IConnectionManager> connectionManagerFactory, System.Func<DbUp.Engine.Output.IUpgradeLog> log, string schema, System.Func<bool> variablesEnabled, System.Collections.Generic.IEnumerable<DbUp.Engine.IScriptPreprocessor> scriptPreprocessors, System.Func<DbUp.Engine.IJournal> journalFactory) { }
        protected override void ExecuteCommandsWithinExceptionHandler(int index, DbUp.Engine.SqlScript script, System.Action executeCommand) { }
        protected override string GetVerifySchemaSql(string schema) { }
    }
}
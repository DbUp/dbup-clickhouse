using DbUp.Builder;
using DbUp.Tests.Common;

namespace DbUp.ClickHouse.Tests;

public class DatabaseSupportTests : DatabaseSupportTestsBase
{
    public DatabaseSupportTests() : base()
    {
    }

    protected override UpgradeEngineBuilder DeployTo(SupportedDatabases to)
        => to.ClickHouseDatabase("");

    protected override UpgradeEngineBuilder AddCustomNamedJournalToBuilder(UpgradeEngineBuilder builder, string schema, string tableName)
        => builder.JournalTo(
            (connectionManagerFactory, logFactory)
                => new ClickHouseJournal(connectionManagerFactory, logFactory, schema, tableName)
        );
}

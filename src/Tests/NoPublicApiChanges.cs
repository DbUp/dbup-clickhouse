using DbUp.Tests.Common;

namespace DbUp.ClickHouse.Tests;

public class NoPublicApiChanges : NoPublicApiChangesBase
{
    public NoPublicApiChanges()
        : base(typeof(ClickHouseExtensions).Assembly)
    {
    }
}

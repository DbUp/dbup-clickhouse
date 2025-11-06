using Testcontainers.ClickHouse;
using Xunit;

namespace DbUp.ClickHouse.Tests;

/// <summary>
/// Class fixture for a ClickHouse container that is shared across all tests in a test class.
/// This ensures the container is created once per test class rather than once per test.
/// </summary>
public class ClickHouseContainerFixture : IAsyncLifetime
{
    private ClickHouseContainer? clickhouseContainer;
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // Create and start a ClickHouse container
        var container = new ClickHouseBuilder().WithDatabase("testdb")
            .Build();

        await container.StartAsync();

        ConnectionString = container.GetConnectionString();
        clickhouseContainer = container;
    }

    public async Task DisposeAsync()
    {
        if (clickhouseContainer != null)
        {
            await clickhouseContainer.DisposeAsync();
        }
    }
}

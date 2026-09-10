using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace UserManagement.IntegrationTests.Api;

// One per test class, so classes that write data cannot see each other.
public sealed class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine").Build();
    private WebApplicationFactory<Program>? _factory;

    public HttpClient Client { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("ConnectionStrings:Default", _container.GetConnectionString()));

        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _factory?.Dispose();
        await _container.DisposeAsync();
    }
}

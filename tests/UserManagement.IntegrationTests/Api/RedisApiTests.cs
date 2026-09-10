using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using UserManagement.Contracts.Common;
using UserManagement.Contracts.Users;

namespace UserManagement.IntegrationTests.Api;

// The other API tests run the cache in-process. This one proves the Redis-backed path end to end.
public class RedisApiTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine").Build();
    private WebApplicationFactory<Program>? _factory;
    private HttpClient _client = default!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", _postgres.GetConnectionString());
            builder.UseSetting("ConnectionStrings:Redis", _redis.GetConnectionString());
        });
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _factory?.Dispose();
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask());
    }

    [Fact]
    public async Task Ready_MustIncludeRedis()
    {
        var response = await _client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Users_MustNotBeStaleAfterAWriteWhenCachedInRedis()
    {
        var before = await _client.GetFromJsonAsync<PagedResult<UserDto>>("/api/v1/users");
        await _client.GetFromJsonAsync<PagedResult<UserDto>>("/api/v1/users");

        var request = new UserRequest { Forename = "Cache", Surname = "Test", Email = "cache@example.com", DateOfBirth = new DateOnly(1990, 1, 1), IsActive = true };
        (await _client.PostAsJsonAsync("/api/v1/users", request)).StatusCode.Should().Be(HttpStatusCode.Created);

        var after = await _client.GetFromJsonAsync<PagedResult<UserDto>>("/api/v1/users");

        after!.TotalCount.Should().Be(before!.TotalCount + 1);
        var keys = await _redis.ExecAsync(["redis-cli", "--scan", "--pattern", "usermanagement:*"]);
        keys.Stdout.Should().Contain("usermanagement:", "the cache should be stored in Redis");
    }
}

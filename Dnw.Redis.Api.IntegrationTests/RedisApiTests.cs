using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using Xunit;

namespace Dnw.Redis.Api.IntegrationTests;

public class RedisApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _apiFactory;

    public RedisApiTests(ApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }
    
    [Fact]
    public async Task GetAsync()
    {
        // Given
        var api = _apiFactory.CreateClient();

        // When
        var actual = await api.GetAsync("/api/redis/key");
        
        // Then
        Assert.NotNull(actual);
    }
}

[UsedImplicitly]
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly RedisTestcontainer _redisContainer = new TestcontainersBuilder<RedisTestcontainer>()
        .WithDatabase(new RedisTestcontainerConfiguration())
        .Build();

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync().ConfigureAwait(false);
    }

    public new async Task DisposeAsync()
    {
        await _redisContainer.StopAsync().ConfigureAwait(false);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IConnectionMultiplexer));
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(_redisContainer.ConnectionString));
        });
    }
}
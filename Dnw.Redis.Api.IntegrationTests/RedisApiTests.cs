using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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
        const string key = "aKey";
        const string value = "aValue";
        
        var api = _apiFactory.CreateClient();
        
        // When
        await api.PostAsync($"/api/redis/{key}/{value}", null);
        var actual = await api.GetAsync($"/api/redis/{key}");
        
        // Then
        Assert.NotNull(actual);
        var result = await actual.Content.ReadAsStringAsync();
        Assert.Equal(value, result);
    }
}

[UsedImplicitly]
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly ILogger<ApiFactory> _logger;

    private readonly RedisTestcontainer _redisContainer = new TestcontainersBuilder<RedisTestcontainer>()
        .WithDatabase(new RedisTestcontainerConfiguration())
        .WithDockerEndpoint(Environment.GetEnvironmentVariable("DOCKER_HOST"))
        .Build();

    public ApiFactory(ILogger<ApiFactory> logger)
    {
        _logger = logger;
    }
    
    public async Task InitializeAsync()
    {
        _logger.LogInformation("DOCKER_HOST={dockerHost}", Environment.GetEnvironmentVariable("DOCKER_HOST"));
        await _redisContainer.StartAsync().ConfigureAwait(false);
    }

    public new async Task DisposeAsync()
    {
        await _redisContainer.StopAsync().ConfigureAwait(false);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _logger.LogInformation("DOCKER_HOST={dockerHost}", Environment.GetEnvironmentVariable("DOCKER_HOST"));
        
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IConnectionMultiplexer));
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(_redisContainer.ConnectionString));
        });
    }
}
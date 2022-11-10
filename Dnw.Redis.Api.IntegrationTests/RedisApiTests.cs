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
using Xunit.Abstractions;

namespace Dnw.Redis.Api.IntegrationTests;

public class RedisApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _apiFactory;
    private readonly ITestOutputHelper _testOutputHelper;

    public RedisApiTests(ApiFactory apiFactory, ITestOutputHelper testOutputHelper)
    {
        _apiFactory = apiFactory;
        _testOutputHelper = testOutputHelper;
    }
    
    //[Fact]
    public async Task GetAsync()
    {
        // Given
        const string key = "aKey";
        const string value = "aValue";
        
        _testOutputHelper.WriteLine($"DOCKER_HOST={Environment.GetEnvironmentVariable("DOCKER_HOST")}");
        
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
    private  RedisTestcontainer? _redisContainer;

    public async Task InitializeAsync()
    {
        var redisContainerBuilder = new TestcontainersBuilder<RedisTestcontainer>().WithDatabase(new RedisTestcontainerConfiguration());

        var dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
        if (!string.IsNullOrWhiteSpace(dockerHost))
        {
            redisContainerBuilder.WithDockerEndpoint(dockerHost);
        }
        else
        {
            redisContainerBuilder.WithDockerEndpoint("unix:///var/run/docker.sock");
        }

        _redisContainer = redisContainerBuilder.Build();
        
        await _redisContainer.StartAsync().ConfigureAwait(false);
    }

    public new async Task DisposeAsync()
    {
        await _redisContainer!.StopAsync().ConfigureAwait(false);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IConnectionMultiplexer));
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(_redisContainer!.ConnectionString));
        });
    }
}
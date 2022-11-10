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

public class RedisApiTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RedisApiTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public async Task GetAsync()
    {
        // Given
        const string key = "aKey";
        const string value = "aValue";
        
        _testOutputHelper.WriteLine($"DOCKER_HOST={Environment.GetEnvironmentVariable("DOCKER_HOST")}");
        
        var redisContainer = new TestcontainersBuilder<RedisTestcontainer>()
            .WithDatabase(new RedisTestcontainerConfiguration())
            .Build();

        await redisContainer.StartAsync()
            .ConfigureAwait(false);

        var apiFactory = new ApiFactory(redisContainer.ConnectionString);
        var api = apiFactory.CreateClient();

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
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    // private  RedisTestcontainer? _redisContainer;
    //
    // public async Task InitializeAsync()
    // {
    //     var redisContainerBuilder = new TestcontainersBuilder<RedisTestcontainer>().WithDatabase(new RedisTestcontainerConfiguration());
    //     _redisContainer = redisContainerBuilder.Build();
    //     
    //     await _redisContainer.StartAsync().ConfigureAwait(false);
    // }
    //
    // public new async Task DisposeAsync()
    // {
    //     await _redisContainer!.StopAsync().ConfigureAwait(false);
    // }

    public ApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IConnectionMultiplexer));
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(_connectionString));
        });
    }
}
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using StackExchange.Redis;
using Xunit;

namespace Dnw.Redis.Api.IntegrationTests;

public class RedisContainerTests
{
    //[Fact]
    public async Task Get()
    {
        // Given
        const string key = "aKey";
        const string value = "aValue";
        
        var redisContainer = new TestcontainersBuilder<RedisTestcontainer>()
                .WithDatabase(new RedisTestcontainerConfiguration())
                .Build();

        await redisContainer.StartAsync()
            .ConfigureAwait(false);
        
        var mux = await ConnectionMultiplexer.ConnectAsync(redisContainer.ConnectionString).ConfigureAwait(false);
        var db = mux.GetDatabase();
        
        // When
        db.StringSet(new RedisKey(key), value);
        var actual = db.StringGet(new RedisKey(key));
        
        // Then
        Assert.Equal(value, actual);
    }
}
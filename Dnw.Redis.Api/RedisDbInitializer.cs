using StackExchange.Redis;

namespace Dnw.Redis.Api;

public class RedisDbInitializer : IHostedService
{
    private readonly IDatabase _db;
    
    public RedisDbInitializer(IConnectionMultiplexer mux)
    {
        _db = mux.GetDatabase();
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _db.StringSet("key", "value");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Dnw.Redis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RedisController : ControllerBase
{
    private readonly IDatabase _db;
    
    public RedisController(IConnectionMultiplexer mux)
    {
        _db = mux.GetDatabase();
    }
    
    [HttpPost("{key}/{value}")]
    public void Get(string key, string value)
    {
        _db.StringSet(new RedisKey(key), value);
    }
    
    [HttpGet("{key}")]
    public string Get(string key)
    {
        return _db.StringGet(new RedisKey(key))!;
    }
}

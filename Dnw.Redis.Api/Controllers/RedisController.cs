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
    
    [HttpGet("{key}")]
    public string Get(string key)
    {
        return _db.StringGet(new RedisKey(key))!;
    }
}

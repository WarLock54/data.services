using Business;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace PostgreCore
{
    [ApiController]
    [Route("[controller]")]
    public class LocationController : DaBDataController<Location, long, LocationBs>
    {
        public LocationController(IRedisService<Location> redisService) : base(redisService)
        {
        }
    };
}

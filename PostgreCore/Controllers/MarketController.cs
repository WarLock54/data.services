using Business;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace PostgreCore
{
    [ApiController]
    [Route("[controller]")]
    public partial class MarketController : DaBDataController<Market, long, MarketBs> {
        public MarketController(IRedisService<Market> redisService) : base(redisService)
        {
        }
    };
}

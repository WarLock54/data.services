using Business;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace PostgreCore
{
    [ApiController]
    [Route("[controller]")]
    public partial class MesajSablonController : DaBDataController<MesajSablon, long, MesajSablonBs>
    {
        public MesajSablonController(IRedisService<MesajSablon> redisService) : base(redisService)
        {
        }
    };
}

using Business;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace PostgreCore.Controllers
{
    public class CustomerController : DaBDataController<Customer, long, CustomerBs>
    {
        public CustomerController(IRedisService<Customer> redisService) : base(redisService)
        {
        }
    };
}

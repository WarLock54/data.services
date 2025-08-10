using Business;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace PostgreCore.Controllers
{
    public partial class MarketController : DaBDataController<Market, long, MarketBs> { };
}

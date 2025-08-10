using Microsoft.AspNetCore.Mvc;
using Model;

namespace PostgreCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        LocationService locationService;
        public LocationController(LocationService _locationService)
        {
            locationService = _locationService;
        }

        [HttpPost("Add")]
        public IActionResult Add(Location location)
        {
            locationService.LocationAdd(location);
            return Ok();
        }

        [HttpPost("FindGeo")]
        public IActionResult FindGeo(FindGeo findGeo)
        {
            // Country : "TR","EN","FR" key in find geo
            return Ok(locationService.Any(findGeo));
        }
    }
}

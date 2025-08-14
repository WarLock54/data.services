using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using PostgreCore.Jwt;

namespace PostgreCore
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DailyJwtKeyProvider _keyProvider; // KeyProvider'ı inject et

        // Constructor'ı güncelle
        public AuthController(IConfiguration configuration, DailyJwtKeyProvider keyProvider)
        {
            _configuration = configuration;
            _keyProvider = keyProvider;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginModel)
        {
            if (loginModel.Username == "admin" && loginModel.Password == "password")
            {
                // O güne ait anahtarı al
                string dailyKey = _keyProvider.GetDailyKey();

                // Helper'a bu anahtarı göndererek token üret
                var token = JwtHelper.GenerateJwtToken(_configuration, loginModel.Username, dailyKey);

                return Ok(new { token });
            }

            return Unauthorized("Kullanıcı adı veya şifre hatalı.");
        }
    }
}

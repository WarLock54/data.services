using System.Text;
using System.Security.Cryptography;
namespace PostgreCore.Jwt
{
    public class DailyJwtKeyProvider
    {
        private readonly IConfiguration _configuration;
        private (string date, string key) _cachedKey; // Basit bir önbellekleme için

        public DailyJwtKeyProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetDailyKey()
        {
            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            // Eğer önbellekteki anahtar bugüne aitse, tekrar hesaplama yapmadan onu döndür.
            if (_cachedKey.date == today && !string.IsNullOrEmpty(_cachedKey.key))
            {
                return _cachedKey.key;
            }

            // MasterKey'i appsettings'den al
            string masterKey = _configuration["Jwt:MasterKey"];
            if (string.IsNullOrEmpty(masterKey))
            {
                throw new ArgumentNullException(nameof(masterKey), "Jwt:MasterKey appsettings.json dosyasında tanımlı değil!");
            }

            // MasterKey ve bugünün tarihini birleştir
            string stringToHash = $"{masterKey}-{today}";

            // SHA256 ile hash'le
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
                // Güvenli bir anahtar olarak kullanmak için Base64 formatına çevir
                string dailyKey = Convert.ToBase64String(hashedBytes);

                // Yeni anahtarı önbelleğe al
                _cachedKey = (today, dailyKey);

                return dailyKey;
            }
        }
    }
}

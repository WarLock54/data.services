using ServiceStack.Redis;
using System.Text.Json;

namespace PostgreCore
{
    public class RedisService<TEntity> : IRedisService<TEntity> where TEntity : class
    {
        private readonly IRedisDbProvider _redisProvider;

        public RedisService(IRedisDbProvider redisProvider)
        {
            _redisProvider = redisProvider ?? throw new ArgumentNullException(nameof(redisProvider));
        }

        private string GetKey(string id) => $"{typeof(TEntity).Name}:{id}";


        private string GetEntityKey(TEntity entity)
        {
            var type = typeof(TEntity);
            var prop = type.GetProperty("Id") ?? type.GetProperty("Kod") ?? type.GetProperty("Key");

            if (prop == null)
                throw new Exception("Entity üzerinde 'Id', 'Kod' veya 'Key' property bulunamadı.");

            var value = prop.GetValue(entity);
            if (value == null)
                throw new Exception("Entity'nin key property'si null.");

            return value.ToString();
        }
        public async Task AddAsync(TEntity entity)
        {
            var key = GetEntityKey(entity);
            if (string.IsNullOrEmpty(key) || key == "0")
                throw new InvalidOperationException("Entity'nin PK değeri boş veya sıfır, önce veritabanına eklenmeli.");

            // Entity'yi JSON string'e çeviriyoruz.
            var jsonValue = JsonSerializer.Serialize(entity);
            // Redis'e string olarak kaydediyoruz.
            await _redisProvider.database.StringSetAsync(key, jsonValue);
        }

        public async Task<TEntity> FindAsync(string id)
        {
            var key = GetKey(id);
            var jsonValue = await _redisProvider.database.StringGetAsync(key);

            if (jsonValue.IsNullOrEmpty)
            {
                throw new InvalidOperationException("Entity'nin PK değeri boş veya sıfır, önce veritabanına eklenmeli.");

            }

            // Redis'ten gelen JSON string'i tekrar Entity nesnesine çeviriyoruz.
            return JsonSerializer.Deserialize<TEntity>(jsonValue!);
        }

        public async Task DeleteAsync(string id)
        {
            var key = GetKey(id);
            await _redisProvider.database.KeyDeleteAsync(key);
        }

        public async Task UpdateAsync(string id, TEntity entity)
        {
            var key = GetKey(id);
            var jsonValue = JsonSerializer.Serialize(entity);

            // Update işlemi de aslında aynı key'e yeni değeri yazmaktır.
            await _redisProvider.database.StringSetAsync(key, jsonValue);
        }
    }
}

using ServiceStack.Redis;

namespace PostgreCore
{
    public class RedisService<TEntity> : IRedisService<TEntity> where TEntity : class
    {
        protected readonly IRedisClientsManager _client;
        public RedisService(IRedisClientsManager client)
        {
            _client = client;
        }
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

            using var redis = _client.GetClient();
             redis.Set(key, entity);
        }

        public async Task<TEntity> FindAsync(string key)
        {
            using var redis = _client.GetClient();
            return  redis.Get<TEntity>(key);
        }

        public async Task DeleteAsync(string key)
        {
            using var redis = _client.GetClient();
             redis.Remove(key);
        }

        public async Task UpdateAsync(string key, TEntity entity)
        {
            using var redis = _client.GetClient();
            redis.Set(key, entity); // Update = Set in Redis
        }
    }
}

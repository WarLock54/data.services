namespace PostgreCore
{
    //redis önbelleği arasınadaki iletişimi sağlar.
    public class RedisCacheHandler : ICacheHandler
    {
        private readonly IRedisDbProvider _redisDbProvider;

        public RedisCacheHandler(IRedisDbProvider redisDbProvider)
        {
            _redisDbProvider = redisDbProvider ??
            throw new ArgumentNullException(nameof(redisDbProvider));

            if (_redisDbProvider.database == null)
            {
                throw new ArgumentNullException("The provided redisDbProvider or its database is null");
            }
        }

        private bool disposedValue;

        public Task StringDeleteAsync(string key)
        {
            var _ = _redisDbProvider.database.StringGetDeleteAsync(key).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        public async Task<bool> StringExistsAsync(string key)
        {
            return await _redisDbProvider.database.KeyExistsAsync(key).ConfigureAwait(false);
        }

        public async Task<string?> StringGetAsync(string key)
        {
            return await _redisDbProvider.database.StringGetAsync(key).ConfigureAwait(false);
        }

        public async Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = null)
        {
            return await _redisDbProvider.database.StringSetAsync(key, value, expiry).ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _redisDbProvider.Dispose();
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        ~RedisCacheHandler()
        {
            Dispose(disposing: false);
        }
    }
}

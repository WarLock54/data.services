using StackExchange.Redis;

namespace PostgreCore
{
    public class RedisDbProvider : IRedisDbProvider
    {
         private bool disposed = false;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        // Constructor'ı DI'dan IConnectionMultiplexer alacak şekilde değiştiriyoruz.
        public RedisDbProvider(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        }
        public IDatabase database => _connectionMultiplexer.GetDatabase();

        public void Dispose()
        {
            // IConnectionMultiplexer Singleton olarak yönetildiği için burada dispose edilmemeli.
            // GC.SuppressFinalize(this);
        }
    }
}

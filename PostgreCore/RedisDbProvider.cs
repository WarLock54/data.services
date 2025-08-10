using StackExchange.Redis;

namespace PostgreCore
{
    public class RedisDbProvider : IRedisDbProvider
    {
         private bool disposed = false;
        private readonly string _connectionString;

        //Bir veya daha fazla Redis sunucusuna yapılan birden fazla bağlantıyı yönetmekten sorumlu bir çoklama katmanı olarak hizmet eder.
        private readonly Lazy<ConnectionMultiplexer> _lazyConnection;
        public RedisDbProvider(string conString)
        {
            conString = _connectionString ??  throw new  ArgumentNullException(conString);
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_connectionString));
        }
        public IDatabase database => _lazyConnection.Value.GetDatabase();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing) {
            if (disposed)
                return;
            if(disposing)
            {
                if(_lazyConnection.IsValueCreated)
                    _lazyConnection.Value.Dispose();
            }
            disposed = true;
           
        }
    }
}

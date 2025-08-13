using StackExchange.Redis;

namespace PostgreCore
{
    public interface IRedisDbProvider : IDisposable
    {
        public IDatabase database { get; }
    }
}

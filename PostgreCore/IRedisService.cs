namespace PostgreCore
{
    public interface IRedisService<TEntity> where TEntity : class
    {
        Task AddAsync(TEntity entity);
        Task<TEntity> FindAsync(string key);
        Task DeleteAsync(string key);
        Task UpdateAsync(string key, TEntity entity);
    }
}

using System.Linq.Expressions;
namespace IdleRPG.Domain.Repositories
{
    public interface IRepository<T>   where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
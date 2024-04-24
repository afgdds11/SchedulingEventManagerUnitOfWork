using System.Linq.Expressions;

namespace SchedulingEventManager.Contract
{
    public interface IRepositoryBase<T>
    {
        Task<IEnumerable<T>> FindAll();
        Task<IEnumerable<T>> FindByCondition(Expression<Func<T, bool>> expression);
        Task Create(T entity);
        Task Update(T entity);
        Task<bool> Delete(int id);
    }
}

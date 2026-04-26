using System.Linq.Expressions;

namespace ApplePaySimulation.Repository.Abstracts
{
    public interface IRepository<T> where T : class
    {
        public Task<List<T>> GetAll();
        public Task<T> GetByID(int id);
        public Task Update(T entity);
        public Task Delete(T entity);
        public Task<int?> Create(T entity);
        public  Task<List<T>> GetAllByFilter(Expression<Func<T, bool>> filter);
    }
}

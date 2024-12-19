using System.Linq.Expressions;

namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface IGenericRepository<T> where T : class
    {
        public Task<IEnumerable<T>> GetAll(Expression<Func<T,bool>>?  filter, int page_size, int page_number, string? includeProperty = null);
        public Task<IEnumerable<T>> GetAll(string? includeProperty = null);
        public Task<T> GetById(int id, string? includeProperty = null);
        public Task Create(T model);
        public void Update(T model);
        public int Delete(int id);
        public Task<bool> IsExist(int id);
        public Task<bool> IsEmpty(Expression<Func<T, bool>>? filter = null);
        public Task<int> Count(Expression<Func<T, bool>>? filter = null);
    }
}

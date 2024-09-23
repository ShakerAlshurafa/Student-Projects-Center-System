namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface IGenericRepository<T> where T : class
    {
        public Task<IEnumerable<T>> GetAll(int page_size, int page_number, string? includeProperty = null);
        public Task<T> GetById(int id);
        public Task Create(T model);
        public void Update(T model);
        public int Delete(int id);
    }
}

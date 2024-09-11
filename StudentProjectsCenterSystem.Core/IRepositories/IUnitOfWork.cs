namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface IUnitOfWork<T> where T : class
    {
        public IProjectRepository projectRepository { get; set; }

        public Task<int> save();
    }
}

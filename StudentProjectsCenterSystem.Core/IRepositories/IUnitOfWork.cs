namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface IUnitOfWork<T> where T : class
    {
        public IProjectRepository projectRepository { get; set; }
        public IProjectDetailsSectionsRepository detailsSectionsRepository { get; set; } // section
        public IProjectDetailsRepository projectDetailsRepository { get; set; } // details

        public Task<int> save();
    }
}

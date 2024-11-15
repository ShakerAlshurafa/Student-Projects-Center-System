namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface IUnitOfWork<T> where T : class
    {
        public IProjectRepository projectRepository { get; set; }
        public IProjectDetailsSectionsRepository detailsSectionsRepository { get; set; } // section
        public IProjectDetailsRepository projectDetailsRepository { get; set; } // details
        public IUserRepository userRepository { get; set; }
        public IWorkgroupRepository workgroupRepository { get; set; }
        public ITaskRepository taskRepository { get; set; }

        public Task<int> save();
    }
}

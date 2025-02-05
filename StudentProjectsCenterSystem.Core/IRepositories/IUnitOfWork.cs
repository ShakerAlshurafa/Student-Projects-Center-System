using StudentProjectsCenter.Core.IRepositories;

namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface IUnitOfWork
    {
        public IProjectRepository projectRepository { get; set; }
        public IProjectDetailsSectionsRepository detailsSectionsRepository { get; set; } // section
        public IProjectDetailsRepository projectDetailsRepository { get; set; } // details
        public IUserRepository userRepository { get; set; }
        public IWorkgroupRepository workgroupRepository { get; set; }
        public ITaskRepository taskRepository { get; set; }
        public IFileRepository fileRepository { get; set; }
        public ITermRepository termRepository { get; set; }
        public IMessageRepository messageRepository { get; set; }
        public ICelenderRepository celenderRepository { get; set; }

        public Task<int> save();
    }
}

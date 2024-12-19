using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenter.Infrastructure.Repositories;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;

namespace StudentProjectsCenterSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext dbContext;

        public IProjectRepository projectRepository { get; set; }
        public IProjectDetailsSectionsRepository detailsSectionsRepository { get; set; }
        public IProjectDetailsRepository projectDetailsRepository { get; set; }
        public IUserRepository userRepository { get; set; }
        public IWorkgroupRepository workgroupRepository { get; set; }
        public ITaskRepository taskRepository { get; set; }
        public ITermRepository termRepository { get; set; }
        public IMessageRepository messageRepository { get; set; }


        public UnitOfWork(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            projectRepository = new ProjectRepository(dbContext);
            detailsSectionsRepository = new ProjectDetailsSectionsRepository(dbContext);
            projectDetailsRepository = new ProjectDetailsRepository(dbContext);
            userRepository = new UserRepository(dbContext);
            workgroupRepository = new WorkgroupRepository(dbContext);
            taskRepository = new TaskRepository(dbContext);
            termRepository = new TermRepository(dbContext);
            messageRepository = new MessageRepository(dbContext);
        }

        public async Task<int> save() => await dbContext.SaveChangesAsync();
    }
}

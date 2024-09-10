using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;

namespace StudentProjectsCenterSystem.Infrastructure.Repositories
{
    public class UnitOfWork<T> : IUnitOfWork<T> where T : class
    {
        private readonly ApplicationDbContext dbContext;

        public IProjectRepository projectRepository { get; set; }


        public UnitOfWork(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            projectRepository = new ProjectRepository(dbContext);
        }

        public async Task<int> save() => await dbContext.SaveChangesAsync();
    }
}

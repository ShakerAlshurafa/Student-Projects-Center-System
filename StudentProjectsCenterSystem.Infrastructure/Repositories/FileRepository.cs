using StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task;
using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;
using StudentProjectsCenterSystem.Infrastructure.Repositories;

namespace StudentProjectsCenter.Infrastructure.Repositories
{
    public class FileRepository : GenericRepository<WorkgroupFile>, IFileRepository
    {
        private readonly ApplicationDbContext dbContext;

        public FileRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}

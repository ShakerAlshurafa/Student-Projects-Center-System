using StudentProjectsCenter.Core.Entities.Domain.workgroup;
using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;
using StudentProjectsCenterSystem.Infrastructure.Repositories;

namespace StudentProjectsCenter.Infrastructure.Repositories
{
    public class CelenderRepository : GenericRepository<Celender>, ICelenderRepository
    {
        private readonly ApplicationDbContext dbContext;

        public CelenderRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}

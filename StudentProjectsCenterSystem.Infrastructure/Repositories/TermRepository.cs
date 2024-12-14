using StudentProjectsCenter.Core.Entities.Domain.Terms;
using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;
using StudentProjectsCenterSystem.Infrastructure.Repositories;

namespace StudentProjectsCenter.Infrastructure.Repositories
{
    public class TermRepository : GenericRepository<Term>, ITermRepository
    {
        private readonly ApplicationDbContext dbContext;

        public TermRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}

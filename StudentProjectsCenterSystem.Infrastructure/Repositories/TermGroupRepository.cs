using StudentProjectsCenter.Core.Entities.Domain.Terms;
using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;
using StudentProjectsCenterSystem.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenter.Infrastructure.Repositories
{
    public class TermGroupRepository : GenericRepository<TermGroup>, ITermGroupRepository
    {
        private readonly ApplicationDbContext dbContext;

        public TermGroupRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}

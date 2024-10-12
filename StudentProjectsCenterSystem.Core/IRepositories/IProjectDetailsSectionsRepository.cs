using Microsoft.EntityFrameworkCore;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;

namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface IProjectDetailsSectionsRepository : IGenericRepository<ProjectDetailsSection>
    {
        public Task<int> GetSectionId (string name);
    }
}

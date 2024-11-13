using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities.project;
using System.Linq.Expressions;

namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        public Task<ProjectDetailsDTO> GetByIdWithDetails(int id);
    }
}

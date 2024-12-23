using StudentProjectsCenterSystem.Core.Entities.project;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetails
{
    public class ProjectDetailEntityDTO
    {
        public string SectionName { get; set; } = string.Empty;
        public List<ProjectDetailEntity> details { get; set; } = new List<ProjectDetailEntity>();
    }
}

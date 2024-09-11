using StudentProjectsCenterSystem.Core.Entities.project;

namespace StudentProjectsCenterSystem.Core.Entities.DTO
{
    public class ProjectCreateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public IEnumerable<ProjectDetailsCreateDTO> ProjectDetails { get; set; } = Enumerable.Empty<ProjectDetailsCreateDTO>();
    }
}

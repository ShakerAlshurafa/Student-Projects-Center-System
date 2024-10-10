using StudentProjectsCenterSystem.Core.Entities.DTO.MyProject;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.Project
{
    public class ProjectUpdateDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? Overview { get; set; } = string.Empty;
        public string? Status { get; set; } = string.Empty;
        public IEnumerable<ProjectDetailsCreateDTO>? ProjectDetails { get; set; } = Enumerable.Empty<ProjectDetailsCreateDTO>();
    }
}

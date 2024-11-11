using StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetails;
using StudentProjectsCenterSystem.Core.Entities.project;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.Project
{
    public class ProjectDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string SupervisorName { get; set; } = string.Empty;
        public string? CoSupervisorName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public List<string> Team { get; set; } = new List<string>();

        public string Status { get; set; } = string.Empty;
        public bool Favorite { get; set; }

        public string Overview { get; set; } = string.Empty;
        public IEnumerable<ProjectDetailEntityDTO> ProjectDetails { get; set; } = Enumerable.Empty<ProjectDetailEntityDTO>();
        
        public DateTime StartDate { get; set; } = new DateTime();
        public DateTime EndDate { get; set; } = new DateTime();
    }
}

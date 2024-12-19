using StudentProjectsCenter.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetails;
using StudentProjectsCenterSystem.Core.Entities.project;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.Project
{
    public class ProjectDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public DateTime? SupervisorJoinAt { get; set; }
        public string SupervisorId { get; set; } = string.Empty;
        public string SupervisorName { get; set; } = string.Empty;
        public DateTime? CoSupervisorJoinAt { get; set; }
        public string CoSupervisorId { get; set; } = string.Empty;
        public string? CoSupervisorName { get; set; } = string.Empty;

        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public List<StudentDTO> Team { get; set; } = new List<StudentDTO>();
        public string Status { get; set; } = string.Empty;
        public string? ChangeStatusNotes { get; set; }
        public DateTime? ChangeStatusAt { get; set; }

        public bool Favorite { get; set; }

        public string Overview { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = new DateTime();
        public DateTime? EndDate { get; set; }
        public IEnumerable<ProjectDetailEntityDTO> ProjectDetails { get; set; } = Enumerable.Empty<ProjectDetailEntityDTO>();
        
    }
}

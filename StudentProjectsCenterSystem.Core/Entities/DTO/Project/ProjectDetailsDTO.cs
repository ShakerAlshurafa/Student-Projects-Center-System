using StudentProjectsCenter.Core.Entities.DTO.Project;
using StudentProjectsCenter.Core.Entities.DTO.Users;
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
        public List<CoSupervisorDTO>? coSupervisors { get; set; }

        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public List<TeamDTO> Team { get; set; } = new List<TeamDTO>();
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

using StudentProjectsCenterSystem.Core.Entities.Domain;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;

namespace StudentProjectsCenterSystem.Core.Entities.project
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Overview { get; set; } = string.Empty;
        public string? Status { get; set; } = "Active"; // Active, Pending, Completed 
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; } = null;

        
        public ICollection<ProjectDetailsSection>? ProjectDetailsSection { get; set; } = new List<ProjectDetailsSection>();

        // Many-to-Many Relationship
        public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();

        // One-to-One Relationship with Workgroup
        public int? WorkgroupId { get; set; }  // Foreign Key
        public Workgroup? Workgroup { get; set; }  // Navigation Property
    }
}

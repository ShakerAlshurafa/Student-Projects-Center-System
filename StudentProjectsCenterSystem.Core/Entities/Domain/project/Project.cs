using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;

namespace StudentProjectsCenterSystem.Core.Entities.project
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Overview { get; set; } = string.Empty;

        public string? Status { get; set; } = "pending"; // Active, Pending, Completed, Canceled
        public string? ChangeStatusNotes { get; set; }
        public DateTime? ChangeStatusAt { get; set; }

        public DateTime? StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; } = null;
        public bool Favorite { get; set; } = false;



        public ICollection<ProjectDetailsSection>? ProjectDetailsSection { get; set; } = new List<ProjectDetailsSection>();

        // Many-to-Many Relationship
        public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();

        // One-to-One Relationship with Workgroup
        public int? WorkgroupId { get; set; }  // Foreign Key
        public Workgroup? Workgroup { get; set; }  // Navigation Property
    }
}

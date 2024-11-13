using StudentProjectsCenterSystem.Core.Entities.project;

namespace StudentProjectsCenterSystem.Core.Entities.Domain.workgroup
{
    public class Workgroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int Progress { get; set; } = 0;

        // One-to-One Relationship with Project
        public Project? Project { get; set; }  // Navigation Property

        // One-to-Many Relationship with Task
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}

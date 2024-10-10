using StudentProjectsCenterSystem.Core.Entities.project;

namespace StudentProjectsCenterSystem.Core.Entities.Domain
{
    public class Workgroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;


        // One-to-One Relationship with Project
        public Project? Project { get; set; }  // Navigation Property
    }
}

using StudentProjectsCenter.Core.Entities.Domain;
using StudentProjectsCenter.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities.project;
using System.Text.Json.Serialization;

namespace StudentProjectsCenterSystem.Core.Entities.Domain.workgroup
{
    public class Workgroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int Progress { get; set; } = 0;

        // One-to-One Relationship with Project
        public Project? Project { get; set; }  // Navigation Property

        // One-to-Many relationship with Message
        public ICollection<Message> Messages { get; set; } = new List<Message>();

        // One-to-Many Relationship with Task
        [JsonIgnore]
        public ICollection<WorkgroupTask> Tasks { get; set; } = new List<WorkgroupTask>();

        // One-to-Many Relationship with Celender
        [JsonIgnore]
        public ICollection<Celender> CelenderEvents { get; set; } = new List<Celender>();
    }
}

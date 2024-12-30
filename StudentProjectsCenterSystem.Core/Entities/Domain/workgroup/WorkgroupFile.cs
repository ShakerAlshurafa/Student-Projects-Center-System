using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using System.Text.Json.Serialization;

namespace StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task
{
    public class WorkgroupFile
    {
        public int Id { get; set; }
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } = string.Empty; // "Question" or "Answer"

        // Foreign key
        [JsonIgnore]
        public int WorkgroupTaskId { get; set; }

        // Navigation property to Workgroup
        [JsonIgnore]
        public WorkgroupTask WorkgroupTask { get; set; } = new WorkgroupTask();
    }
}

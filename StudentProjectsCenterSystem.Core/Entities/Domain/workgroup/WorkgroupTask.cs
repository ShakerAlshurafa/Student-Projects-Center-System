using StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudentProjectsCenterSystem.Core.Entities.Domain.workgroup
{
    public class WorkgroupTask
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Not Started";
        /* Status
            Not Started: Task has been created but work has not yet begun.
            In Progress: Task is currently being worked on.
            Submitted: Task has been completed and submitted for review.
            *Completed: Task is finished.
            *Rejected: Task submission has been reviewed and requires changes or adjustments.
            *Canceled: Task is no longer needed and has been closed.
            Overdue:
        */

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? SubmittedBy { get; set; }
        public string Author { get; set; } = string.Empty;
        public string? LastUpdateBy { get; set; }

        //public List<string> ValidExtensions = new List<string>();

        // Navigation property for WorkgroupFile
        public IEnumerable<WorkgroupFile>? Files { get; set; }

        [JsonIgnore]
        public int WorkgroupId { get; set; }
        [JsonIgnore]
        public Workgroup Workgroup { get; set; } = null!;
    }
}

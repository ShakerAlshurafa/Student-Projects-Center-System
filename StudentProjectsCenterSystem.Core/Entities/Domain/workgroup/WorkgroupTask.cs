using Microsoft.AspNetCore.Http;
using System;
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
            On Hold: Task is temporarily paused.
            Completed: Task is finished.
            Submitted: Task has been completed and submitted for review.
            Approved: Task submission has been reviewed and accepted.
            Rejected: Task submission has been reviewed and requires changes or adjustments.
            Canceled: Task is no longer needed and has been closed.
        */
        public DateTime? Start { get; set; } 
        public DateTime? End { get; set; }

        //public List<string> ValidExtensions = new List<string>();

        public List<string>? QuestionFilePath { get; set; }
        public List<string>? SubmittedFilePath { get; set; }
        //public string? FileName { get; set; }

        [JsonIgnore]
        public int WorkgroupId { get; set; }
        [JsonIgnore]
        public Workgroup Workgroup { get; set; } = null!;
    }
}

using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using System.Text.Json.Serialization;

namespace StudentProjectsCenter.Core.Entities.Domain.workgroup
{
    public class Celender
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public bool AllDay { get; set; } = false;
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }

        public int? WorkgroupId { get; set; }
        public Workgroup Workgroup { get; set; } = null!;
    }
}

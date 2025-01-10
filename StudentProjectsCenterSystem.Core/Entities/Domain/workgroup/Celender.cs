using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;

namespace StudentProjectsCenter.Core.Entities.Domain.workgroup
{
    public class Celender
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public bool AllDay { get; set; } = false;

        //[JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTimeOffset? StartAt { get; set; }

        //[JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTimeOffset? EndAt { get; set; }

        public int? WorkgroupId { get; set; }
        public Workgroup Workgroup { get; set; } = null!;
    }
}

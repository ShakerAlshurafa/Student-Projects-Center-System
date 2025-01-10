using System.Text.Json.Serialization;

namespace StudentProjectsCenter.Core.Entities.DTO.Workgroup
{
    public class CelenderEventDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public bool AllDay { get; set; } = false;

        //[JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTimeOffset? StartAt { get; set; }

        //[JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTimeOffset? EndAt { get; set; }
    }
}

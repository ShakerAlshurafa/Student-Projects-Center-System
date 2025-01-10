using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudentProjectsCenter.Core.Entities.DTO.Workgroup
{
    public class CreateCelenderEventDTO
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool AllDay { get; set; } = false;

        //[JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTimeOffset? StartAt { get; set; }

        //[JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTimeOffset? EndAt { get; set; }
    }
}

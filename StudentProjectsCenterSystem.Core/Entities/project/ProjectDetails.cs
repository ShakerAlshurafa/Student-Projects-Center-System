using System.Text.Json.Serialization;

namespace StudentProjectsCenterSystem.Core.Entities.project
{
    public class ProjectDetails
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;  // Goals, Technology, .....
        public byte[] IconData { get; set; } = Array.Empty<byte>();   // Icon as binary data

        [JsonIgnore]
        public int ProjectId { get; set; }
        [JsonIgnore]
        public Project? Project { get; set; }
    }
}

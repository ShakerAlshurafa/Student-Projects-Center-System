using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using System.Text.Json.Serialization;

namespace StudentProjectsCenterSystem.Core.Entities.project
{
    public class ProjectDetailEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public byte[]? IconData { get; set; } = Array.Empty<byte>();   // Icon as binary data



        // Foreign key to ProjectDetailsSection
        [JsonIgnore]
        public int ProjectDetailsSectionId { get; set; }
        // Navigation property for the section
        [JsonIgnore]
        public ProjectDetailsSection? ProjectDetailsSection { get; set; }
    }
}

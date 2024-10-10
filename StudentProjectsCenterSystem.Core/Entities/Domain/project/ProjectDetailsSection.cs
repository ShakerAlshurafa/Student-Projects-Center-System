using StudentProjectsCenterSystem.Core.Entities.project;
using System.Text.Json.Serialization;

namespace StudentProjectsCenterSystem.Core.Entities.Domain.project
{
    public class ProjectDetailsSection
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;


        [JsonIgnore]
        public int ProjectId { get; set; }
        [JsonIgnore]
        public Project? Project { get; set; }

        // Navigation property for one-to-many relationship
        public ICollection<ProjectDetailEntity>? ProjectDetails { get; set; }
    }
}

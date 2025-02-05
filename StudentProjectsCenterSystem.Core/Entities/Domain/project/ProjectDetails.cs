using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudentProjectsCenterSystem.Core.Entities.project
{
    public class ProjectDetailEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; }


        // Foreign key to ProjectDetailsSection
        [JsonIgnore]
        public int ProjectDetailsSectionId { get; set; }
        // Navigation property for the section
        [JsonIgnore]
        public ProjectDetailsSection? ProjectDetailsSection { get; set; }
    }
}

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.MyProject
{
    public class ProjectDetailsCreateDTO
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;
        public IFormFile? image { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenter.Core.Entities.DTO.Terms
{
    public class TermGroupCreateDTO
    {
        [Required(ErrorMessage = "The Title field is required.")]
        [StringLength(100, ErrorMessage = "The Title must be at most 100 characters long.")]
        public string Title { get; set; } = string.Empty;
    }
}

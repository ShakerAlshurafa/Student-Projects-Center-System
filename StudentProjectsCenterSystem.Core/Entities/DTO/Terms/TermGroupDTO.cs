using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenter.Core.Entities.DTO.Terms
{
    public class TermGroupDTO
    {
        [Required(ErrorMessage = "Id is required.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; } = string.Empty;
    }
}

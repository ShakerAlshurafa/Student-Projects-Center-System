using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenter.Core.Entities.DTO.Terms
{
    public class TermDTO
    {
        [Required(ErrorMessage = "Description is required.")]
        public List<string> Description { get; set; } = new List<string>();
    }
}

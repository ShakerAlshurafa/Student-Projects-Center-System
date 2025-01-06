using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenter.Core.Entities.DTO.Workgroup
{
    public class CreateCelenderEventDTO
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool AllDay { get; set; } = false;
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
    }
}

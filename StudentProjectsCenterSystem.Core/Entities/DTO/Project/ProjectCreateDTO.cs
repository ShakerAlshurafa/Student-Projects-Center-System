using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.Project
{
    public class ProjectCreateDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string SupervisorId { get; set; } = string.Empty;
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        public string? CompanyName {  get; set; }
    }
}

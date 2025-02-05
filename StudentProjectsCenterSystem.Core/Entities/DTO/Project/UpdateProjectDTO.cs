using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenter.Core.Entities.DTO.Project
{
    public class UpdateProjectDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string SupervisorId { get; set; } = string.Empty;
        public string? ChangeOldSupervisorNotes { get; set; } = string.Empty;
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        public string? ChangeOldCustomerNotes { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; } = string.Empty;
        public string? ChangeStatusNotes { get; set; } = string.Empty;
        public string? CompanyName { get; set; }

    }
}

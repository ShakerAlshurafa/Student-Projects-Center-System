using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenter.Core.Entities.DTO.Project
{
    public class UpdateProjectDTO
    {
        public string Name { get; set; } = string.Empty;
        public string SupervisorId { get; set; } = string.Empty;
        public string ChangeOldSupervisorNotes {  get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string ChangeOldCustomerNotes { get; set; } = string.Empty;
        public string? Status { get; set; }
        public string? ChangeStatusNotes { get; set; }

    }
}

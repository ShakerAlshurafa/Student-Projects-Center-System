using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.Authentication
{
    public class ResetPasswordDTO
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string newPassword { get; set; } = string.Empty;
        [Required]
        public string confirmNewPassword { get; set; } = string.Empty;
    }
}

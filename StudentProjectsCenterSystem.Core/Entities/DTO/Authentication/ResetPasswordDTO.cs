using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.Authentication
{
    public class ResetPasswordDTO
    {
        public string Email { get; set; } = string.Empty;
        public string newPassword { get; set; } = string.Empty;
        public string confirmNewPassword { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}

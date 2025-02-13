﻿using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.Authentication
{
    public class ResetPasswordDTO
    {
        [Required]
        public string newPassword { get; set; } = string.Empty;
        [Required]
        public string confirmNewPassword { get; set; } = string.Empty;
    }
}

﻿using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenter.Core.Entities.DTO.Authentication
{
    public class ResetForgetPasswordDTO
    {
        [Required]
        public string ResetCode { get; set; } = string.Empty;
        [Required]
        public string newPassword { get; set; } = string.Empty;
        [Required]
        public string confirmNewPassword { get; set; } = string.Empty;
    }
}

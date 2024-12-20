using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenter.Core.Entities.DTO
{
    public class ContactUsDto
    {

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Message { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(15, MinimumLength = 10)]
        [Phone]
        public string Phone { get; set; } = null!;


    }

}

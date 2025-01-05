using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenter.Core.Entities.DTO.Messages
{
    public class SendMessageDTO
    {
        //[Required]
        //public string WorkgroupName { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenter.Core.Entities.DTO.Message
{
    public class MessageDTO
    {
        public string WorkgroupName { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsUserMessage { get; set; } // To identify user messages
    }
}

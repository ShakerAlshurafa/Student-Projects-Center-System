using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenter.Core.Entities.Domain
{
    public class Message
    {
        public int Id { get; set; }
        public string User { get; set; } = string.Empty;    
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }


        // One-to-One Relationship with Workgroup
        public int? WorkgroupId { get; set; }  // Foreign Key
        public Workgroup? Workgroup { get; set; }  // Navigation Property
    }
}

using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;

namespace StudentProjectsCenter.Core.Entities.Domain
{
    public class Message
    {
        public int Id { get; set; }
        public string User { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }


        // Many-to-One relationship with Workgroup
        public int? WorkgroupId { get; set; }  // Foreign Key
        public Workgroup? Workgroup { get; set; }  // Navigation Property
    }
}

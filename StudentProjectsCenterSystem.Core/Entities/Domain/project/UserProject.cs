using StudentProjectsCenterSystem.Core.Entities.project;

namespace StudentProjectsCenterSystem.Core.Entities.Domain.project
{
    public class UserProject
    {
        public string UserId { get; set; } = string.Empty;
        public LocalUser User { get; set; } = null!;

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public string Role { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;
        public string? DeletedNotes { get; set; }
        public DateTime? DeletededAt { get; set; }
        public DateTime? JoinAt { get; set; } = DateTime.UtcNow;
    }
}

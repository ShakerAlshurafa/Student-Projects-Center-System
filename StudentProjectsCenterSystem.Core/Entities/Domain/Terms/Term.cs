namespace StudentProjectsCenter.Core.Entities.Domain.Terms
{
    public class Term
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set;}  
        public DateTime? LastUpdatedAt { get; set; }
    }
}

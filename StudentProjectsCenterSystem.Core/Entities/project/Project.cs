namespace StudentProjectsCenterSystem.Core.Entities.project
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public ICollection<ProjectDetails> ProjectDetails { get; set; } = new List<ProjectDetails>();
    }
}

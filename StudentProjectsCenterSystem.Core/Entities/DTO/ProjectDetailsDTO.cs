using StudentProjectsCenterSystem.Core.Entities.project;

namespace StudentProjectsCenterSystem.Core.Entities.DTO
{
    public class ProjectDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public IEnumerable<ProjectDetails> ProjectDetails { get; set; } = Enumerable.Empty<ProjectDetails>();
        public DateTime StartDate { get; set; } = new DateTime();
        public DateTime EndDate { get; set; } = new DateTime();
    }
}

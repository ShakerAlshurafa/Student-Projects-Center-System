namespace StudentProjectsCenter.Core.Entities.DTO.ProjectDetails
{
    public class ProjectDetailDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
    }
}

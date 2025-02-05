using StudentProjectsCenter.Core.Entities.DTO.ProjectDetails;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetails
{
    public class ProjectDetailEntityDTO
    {
        public int? SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public List<ProjectDetailDTO> details { get; set; } = new List<ProjectDetailDTO>();
    }
}

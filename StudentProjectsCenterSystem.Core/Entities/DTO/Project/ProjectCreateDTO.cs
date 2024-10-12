namespace StudentProjectsCenterSystem.Core.Entities.DTO.Project
{
    public class ProjectCreateDTO
    {
        public string Name { get; set; } = string.Empty;
        public string SupervisorId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
    }
}

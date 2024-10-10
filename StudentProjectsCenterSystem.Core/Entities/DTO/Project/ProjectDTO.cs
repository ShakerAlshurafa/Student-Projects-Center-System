namespace StudentProjectsCenterSystem.Core.Entities.DTO.Project
{
    public class ProjectDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SupervisorName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string WorkgroupName { get; set; } = string.Empty;
        public List<string> Team { get; set; } = new List<string>();
        public bool Favorite { get; set; } = false;
    }
}

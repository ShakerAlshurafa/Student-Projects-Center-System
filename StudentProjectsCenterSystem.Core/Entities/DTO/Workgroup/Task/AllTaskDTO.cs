namespace StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task
{
    public class AllTaskDTO
    {
        public int Id { get; set; }
        public int WorkgroupId { get; set; }
        public string WorkgroupName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}

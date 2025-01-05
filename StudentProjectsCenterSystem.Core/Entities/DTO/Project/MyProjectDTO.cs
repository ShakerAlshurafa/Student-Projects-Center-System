namespace StudentProjectsCenterSystem.Core.Entities.DTO.Project
{
    public class MyProjectDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
    }
}

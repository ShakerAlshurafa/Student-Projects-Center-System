namespace StudentProjectsCenterSystem.Core.Entities.project
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}

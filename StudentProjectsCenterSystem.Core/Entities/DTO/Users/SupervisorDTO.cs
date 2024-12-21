namespace StudentProjectsCenter.Core.Entities.DTO.Users
{
    public class SupervisorDTO
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> ProjectsName { get; set; } = new List<string>();

    }
}

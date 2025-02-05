namespace StudentProjectsCenter.Core.Entities.DTO.Users
{
    public class UserDTO
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string> Role { get; set; } = new List<string>();
    }
}

namespace StudentProjectsCenterSystem.Core.Entities.DTO
{
    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public List<string> Role { get; set; } = new List<string>();
        public LocalUserDTO User { get; set; } = new LocalUserDTO();
    }
}

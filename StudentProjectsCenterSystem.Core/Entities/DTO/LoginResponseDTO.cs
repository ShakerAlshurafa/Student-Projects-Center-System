namespace StudentProjectsCenterSystem.Core.Entities.DTO
{
    public class LoginResponseDTO
    {
        public LocalUserDTO User { get; set; } = new LocalUserDTO();
        public List<string> Role { get; set; } = new List<string>();
        public bool IsSuccess { get; set; } = true;
        public string? ErrorMessage { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}

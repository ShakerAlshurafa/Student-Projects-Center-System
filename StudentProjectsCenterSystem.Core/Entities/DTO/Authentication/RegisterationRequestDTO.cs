using System.Text.Json.Serialization;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.Authentication
{

    public class RegisterationRequestDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? CompanyName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty; // Select between Customer or Student
    }
}

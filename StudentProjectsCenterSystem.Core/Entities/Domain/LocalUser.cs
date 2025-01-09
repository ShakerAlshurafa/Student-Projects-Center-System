using Microsoft.AspNetCore.Identity;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;

namespace StudentProjectsCenterSystem.Core.Entities
{
    public class LocalUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? CompanyName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string? Address {  get; set; }

        // Stores the 6-digit reset code
        public string? PasswordResetCode { get; set; } 
        // Stores the expiration time of the reset code
        public DateTime? PasswordResetCodeExpiration { get; set; } 


        // Many-to-Many Relationship
        public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();

    }
}

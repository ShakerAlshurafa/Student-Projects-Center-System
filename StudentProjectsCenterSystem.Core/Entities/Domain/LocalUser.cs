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

        // Many-to-Many Relationship
        public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();

    }
}

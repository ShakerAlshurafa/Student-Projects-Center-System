using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Core.Entities
{
    public class LocalUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? CompanyName { get; set; } = string.Empty;
    }
}

using StudentProjectsCenterSystem.Core.Entities.project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Core.Entities.Domain.project
{
    public class UserProject
    {
        public string UserId { get; set; } = string.Empty;
        public LocalUser User { get; set; } = null!;

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public string Role { get; set; } = string.Empty;
    }
}

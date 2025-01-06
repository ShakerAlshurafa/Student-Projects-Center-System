using StudentProjectsCenter.Core.Entities.DTO.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.Workgroup
{
    public class WorkgroupDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Role { get; set; }
        public int Progress { get; set; } = 0;
        public int? ProjectId { get; set; }

        public List<WorkgroupUsersDTO> Members { get; set; } = new List<WorkgroupUsersDTO>();

    }
}

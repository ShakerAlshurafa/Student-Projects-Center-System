using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenter.Core.Entities.DTO.Workgroup
{
    public class AllWorkgroupsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public string SupervisorName { get; set; } = string.Empty;
        public List<string?> CoSupervisorName { get; set; } 
        public string CustomerName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public List<string> Team { get; set; } = new List<string>();
    }
}

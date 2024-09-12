using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Core.Entities.DTO
{
    public class ProjectUpdateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public IEnumerable<ProjectDetailsCreateDTO> ProjectDetails { get; set; } = Enumerable.Empty<ProjectDetailsCreateDTO>();
    }
}

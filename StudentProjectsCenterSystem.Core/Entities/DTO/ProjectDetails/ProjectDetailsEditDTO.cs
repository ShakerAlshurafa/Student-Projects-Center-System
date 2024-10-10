using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetails
{
    public class ProjectDetailsEditDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public byte[] IconData { get; set; } = Array.Empty<byte>();
    }
}

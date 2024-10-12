using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.MyProject
{
    public class ProjectDetailsCreateDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public byte[]? IconData { get; set; } = Array.Empty<byte>();
    }
}

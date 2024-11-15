using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.Workgroup
{
    public class TaskDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? Start { get; set; } = new DateTime();
        public DateTime? End { get; set; } = new DateTime();
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenter.Core.Entities.DTO.Project
{
    public class ChangeProjectStatusDTO
    {
        public string status { get; set; } = string.Empty;
        public string notes { get; set; } = string.Empty;
    }
}

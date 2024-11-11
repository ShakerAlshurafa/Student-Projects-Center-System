using StudentProjectsCenterSystem.Core.Entities.project;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetails
{
    public class ProjectDetailEntityDTO
    {
        public ProjectDetailEntity detail { get; set; } = new ProjectDetailEntity();
        public string SectionName { get; set; } = string.Empty;
    }
}

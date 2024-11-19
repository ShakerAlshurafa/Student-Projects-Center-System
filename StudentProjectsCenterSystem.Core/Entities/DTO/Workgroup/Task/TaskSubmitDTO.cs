using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenter.Core.Entities.DTO.Workgroup
{
    public class TaskSubmitDTO
    {
        public IFormFile? File { get; set; }
    }
}

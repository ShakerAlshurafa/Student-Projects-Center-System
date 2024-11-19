using Microsoft.AspNetCore.Http;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.Workgroup
{
    public class TaskCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        //public string Status { get; set; } = string.Empty;
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        //public List<string> ValidExtensions = new List<string>();
        public IFormFile? File { get; set; } 
    }
}

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.Workgroup
{
    public class TaskCreateDTO
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        //public List<string> ValidExtensions = new List<string>();
        public List<IFormFile>? QuestionFile { get; set; } 
    }
}

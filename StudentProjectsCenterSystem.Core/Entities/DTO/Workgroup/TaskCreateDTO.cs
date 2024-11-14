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
        public List<string> ValidExtensions = new List<string>()
        { 
            // Image Files
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".svg", ".webp",
    
            // Document Files
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".rtf",
    
            // Compressed Files
            ".zip", ".rar", ".7z", ".tar", ".gz",
    
            // Audio Files
            ".mp3", ".wav", ".aac", ".ogg", ".flac",
    
            // Video Files
            ".mp4", ".avi", ".mov", ".wmv", ".mkv", ".flv",
    
            // Code and Data Files
            ".csv", ".json", ".xml", ".html", ".css", ".js", ".sql"
        };
        public IFormFile? File { get; set; } 
    }
}

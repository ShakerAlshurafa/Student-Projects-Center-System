using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Core.Entities.Domain.workgroup
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Start { get; set; } = new DateTime();
        public DateTime End { get; set; } = new DateTime();

        public List<string> ValidExtensions = new List<string>();
        /* Extentions
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
         */

        
        public int WorkgroupId { get; set; }
        public Workgroup Workgroup { get; set; } = null!;
    }
}

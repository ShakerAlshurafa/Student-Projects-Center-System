using Microsoft.AspNetCore.Http;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup;

namespace StudentProjectsCenterSystem.Infrastructure.Utilities
{
    public class UploadHandler
    {
        private readonly List<string> _validExtensions = new List<string>()
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
        private const long MaxFileSize = 50 * 1024 * 1024; // 50 MB
        private readonly string _uploadDirectory;

        public UploadHandler()
        {
            // Refactor to save in the cloud 
            _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Files");

            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }
        }

        public UploadHandler(List<string> validExtensions) : this()
        {
            if (validExtensions is null || validExtensions.Count == 0)
                throw new ArgumentException("Valid extensions cannot be null or empty.", nameof(validExtensions));

            _validExtensions = validExtensions;
       
        }

        public async Task<FileDTO> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new FileDTO("File is empty.");
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_validExtensions.Contains(extension))
            {
                return new FileDTO($"Invalid extension. Allowed extensions are: {string.Join(", ", _validExtensions)}");
            }

            if (file.Length > MaxFileSize)
            {
                return new FileDTO("File size exceeds the 50MB limit.");
            }

            FileDTO fileDto = new FileDTO();
            fileDto.FileName = $"{Guid.NewGuid()}{extension}";
            fileDto.FilePath = Path.Combine(_uploadDirectory, fileDto.FileName);

            try
            {
                await using FileStream stream = new(fileDto.FilePath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                return new FileDTO($"File upload failed: {ex.Message}");
            }


            return fileDto;
        }
    }
}

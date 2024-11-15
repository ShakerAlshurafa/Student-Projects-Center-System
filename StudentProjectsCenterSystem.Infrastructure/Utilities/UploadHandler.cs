using Microsoft.AspNetCore.Http;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup;

namespace StudentProjectsCenterSystem.Infrastructure.Utilities
{
    public class UploadHandler
    {
        private readonly List<string> _validExtensions;
        private const long MaxFileSize = 50 * 1024 * 1024; // 50 MB
        private readonly string _uploadDirectory;

        public UploadHandler(List<string> validExtensions)
        {
            if (validExtensions is null || validExtensions.Count == 0)
                throw new ArgumentException("Valid extensions cannot be null or empty.", nameof(validExtensions));

            _validExtensions = validExtensions;
            // Refactor to save in the cloud 
            _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Files");

            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }
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

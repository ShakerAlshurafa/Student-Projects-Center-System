using Microsoft.AspNetCore.Http;

namespace StudentProjectsCenterSystem.Infrastructure.Utilities
{
    public class UploadHandler
    {
        private readonly List<string> _validExtensions;
        private const long MaxFileSize = 20 * 1024 * 1024; // 20 MB
        private readonly string _uploadDirectory;

        public UploadHandler(List<string> validExtensions)
        {
            if (validExtensions is null || validExtensions.Count == 0)
                throw new ArgumentException("Valid extensions cannot be null or empty.", nameof(validExtensions));

            _validExtensions = validExtensions;
            _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Files");

            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return "File is empty.";
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_validExtensions.Contains(extension))
            {
                return $"Invalid extension. Allowed extensions are: {string.Join(", ", _validExtensions)}";
            }

            if (file.Length > MaxFileSize)
            {
                return "File size exceeds the 20MB limit.";
            }

            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(_uploadDirectory, uniqueFileName);

            try
            {
                await using FileStream stream = new(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                return $"File upload failed: {ex.Message}";
            }

            return uniqueFileName;
        }
    }
}

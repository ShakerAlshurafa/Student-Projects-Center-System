using Azure.Storage.Blobs;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task;

public class AzureFileUploader
{
    private readonly IReadOnlyList<string> _validExtensions;
    private const long MaxFileSize = 50 * 1024 * 1024; // 50 MB
    private readonly string _connectionString;

    public AzureFileUploader(IConfiguration configuration)
    {
        _connectionString = configuration["AzureStorage:ConnectionString"]
                            ?? throw new ArgumentException("Connection string cannot be null or empty.");

        // Set default valid extensions
        _validExtensions = new List<string>
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".svg", ".webp",
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".rtf",
            ".zip", ".rar", ".7z", ".tar", ".gz",
            ".mp3", ".wav", ".aac", ".ogg", ".flac",
            ".mp4", ".avi", ".mov", ".wmv", ".mkv", ".flv",
            ".csv", ".json", ".xml", ".html", ".css", ".js", ".sql"
        };
    }

    public async Task<FileDTO> UploadAsync(IFormFile file, string containerName)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_validExtensions.Contains(extension))
            throw new ArgumentException($"Invalid extension. Allowed extensions: {string.Join(", ", _validExtensions)}");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("File size exceeds the 50MB limit.");

        string fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}{extension}";

        try
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return new FileDTO
            {
                Name = Path.GetFileNameWithoutExtension(file.FileName),
                Path = blobClient.Uri.ToString()
            };
        }
        catch (Exception ex)
        {
            return new FileDTO
            {
                ErrorMessage = ex.Message,
            };
        }
    }
}

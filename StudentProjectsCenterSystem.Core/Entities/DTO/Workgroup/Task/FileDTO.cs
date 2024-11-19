namespace StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task
{
    public class FileDTO
    {
        public FileDTO()
        {
            FileName = string.Empty;
            FilePath = string.Empty;
        }

        public FileDTO(string errorMessage) : this()
        {
            ErrorMessage = errorMessage;
        }

        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

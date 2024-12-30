namespace StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task
{
    public class FileDTO
    {
        public FileDTO()
        {
            Name = string.Empty;
            Path = string.Empty;
        }

        public FileDTO(string errorMessage) : this()
        {
            ErrorMessage = errorMessage;
        }
        public string? Path { get; set; }
        public string? Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Type { get; set; }  // "Question" or "Answer"
        public string? ErrorMessage { get; set; }
    }
}

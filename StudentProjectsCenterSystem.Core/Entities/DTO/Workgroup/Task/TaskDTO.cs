using StudentProjectsCenter.Core.Entities.DTO.Workgroup.Task;

namespace StudentProjectsCenterSystem.Core.Entities.DTO.Workgroup
{
    public class TaskDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public List<FileDTO>? QuestionFiles { get; set; }
        public string Author { get; set; } = string.Empty;
        public DateTime? LastUpdatedAt { get; set; }
        public string? LastUpdateBy { get; set; }

        public List<FileDTO>? AnswerFiles { get; set; }
        public string? SubmittedBy { get; set; }
        public DateTime? SubmittedAt { get; set; }

    }
}

using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenter.Core.Entities.DTO.Messages
{
    public class SendMessageDTO
    {
        [Required]
        public string Message { get; set; } = string.Empty;
    }
}

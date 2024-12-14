using System.Text.Json.Serialization;

namespace StudentProjectsCenter.Core.Entities.Domain.Terms
{
    public class Term
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;

        [JsonIgnore]
        public int TermGroupId { get; set; }
        [JsonIgnore]
        public TermGroup? TermGroup { get; set; }
    }
}

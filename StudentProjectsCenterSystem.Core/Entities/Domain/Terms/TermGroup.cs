namespace StudentProjectsCenter.Core.Entities.Domain.Terms
{
    public class TermGroup
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        
        public ICollection<Term>? Terms { get; set; }
    }
}

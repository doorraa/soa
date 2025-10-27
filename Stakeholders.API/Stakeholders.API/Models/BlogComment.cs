namespace Stakeholders.API.Models
{
    public class BlogComment
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Blog Blog { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}

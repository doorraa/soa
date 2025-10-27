namespace Stakeholders.API.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<string> ImageUrls { get; set; } = new List<string>();

        // Navigation properties
        public User User { get; set; } = null!;
        public List<BlogComment> Comments { get; set; } = new List<BlogComment>();
    }
}

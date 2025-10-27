namespace Stakeholders.API.DTOs
{
    public class BlogDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public int CommentCount { get; set; }
    }
}

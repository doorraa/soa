namespace Stakeholders.API.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Motto { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;

        // Navigation property
        public User User { get; set; } = null!;
    }
}

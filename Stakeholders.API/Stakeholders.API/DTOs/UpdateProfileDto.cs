namespace Stakeholders.API.DTOs
{
    public class UpdateProfileDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Motto { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
    }
}

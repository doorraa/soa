using Stakeholders.API.Models;

namespace Stakeholders.API.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}

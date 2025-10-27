﻿namespace Stakeholders.API.Models
{
    public enum UserRole
    {
        Tourist,
        Guide,
        Admin
    }
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public UserProfile? Profile { get; set; }
    }
}

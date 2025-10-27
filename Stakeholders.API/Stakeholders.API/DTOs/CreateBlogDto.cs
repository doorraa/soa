using System.ComponentModel.DataAnnotations;

namespace Stakeholders.API.DTOs
{
    public class CreateBlogDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(10)]
        public string Description { get; set; } = string.Empty;

        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}

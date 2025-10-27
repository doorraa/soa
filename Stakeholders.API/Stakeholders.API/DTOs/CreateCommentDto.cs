using System.ComponentModel.DataAnnotations;

namespace Stakeholders.API.DTOs
{
    public class CreateCommentDto
    {
        [Required]
        [MinLength(1)]
        [MaxLength(500)]
        public string Content { get; set; } = string.Empty;
    }
}

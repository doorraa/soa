using System.ComponentModel.DataAnnotations;

namespace Tours.API.DTOs
{
    public class PublishTourDto
    {
        [Required]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }
    }
}

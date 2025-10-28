using System.ComponentModel.DataAnnotations;

namespace Tours.API.DTOs
{
    public class AddToCartDto
    {
        [Required]
        public string TourId { get; set; } = string.Empty;
    }
}

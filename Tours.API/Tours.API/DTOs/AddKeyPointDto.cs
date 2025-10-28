using System.ComponentModel.DataAnnotations;

namespace Tours.API.DTOs
{
    public class AddKeyPointDto
    {
        [Required]
        [MinLength(3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Required]
        public int Order { get; set; }
    }
}

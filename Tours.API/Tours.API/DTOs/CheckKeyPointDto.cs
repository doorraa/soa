using System.ComponentModel.DataAnnotations;

namespace Tours.API.DTOs
{
    public class CheckKeyPointDto
    {
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }
    }
}

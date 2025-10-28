using System.ComponentModel.DataAnnotations;
using static Tours.API.Models.Enums;

namespace Tours.API.DTOs
{
    public class CreateTourDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MinLength(10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public TourDifficulty Difficulty { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public double DurationHours { get; set; } = 1.0;
    }
}

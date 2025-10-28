using System.ComponentModel.DataAnnotations;

namespace Tours.API.DTOs
{
    public class StartTourExecutionDto
    {
        [Required]
        public string TourId { get; set; } = string.Empty;
    }
}

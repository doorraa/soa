using Tours.API.Models;

namespace Tours.API.DTOs
{
    public class TourExecutionDto
    {
        public string Id { get; set; } = string.Empty;
        public string TourId { get; set; } = string.Empty;
        public string TourName { get; set; } = string.Empty;
        public ExecutionStatus Status { get; set; }
        public int CompletedKeyPointsCount { get; set; }
        public int TotalKeyPointsCount { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime LastActivity { get; set; }
    }
}

using static Tours.API.Models.Enums;

namespace Tours.API.DTOs
{
    public class TourDto
    {
        public string Id { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TourDifficulty Difficulty { get; set; }
        public TourStatus Status { get; set; }
        public decimal Price { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public int KeyPointsCount { get; set; }
        public KeyPointDto? StartPoint { get; set; }
        public KeyPointDto? EndPoint { get; set; }
        public double DurationHours { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

namespace Tours.API.Models
{
    public class Enums
    {
        public enum TourStatus
        {
            Draft,      // Draft status, cena = 0
            Published,  // Objavljena tura
            Archived    // Arhivirana tura
        }

        public enum TourDifficulty
        {
            Easy,
            Medium,
            Hard
        }
    }
}

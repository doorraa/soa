namespace Tours.API.DTOs
{
    public class PurchaseTokenDto
    {
        public string Id { get; set; } = string.Empty;
        public string TourId { get; set; } = string.Empty;
        public string TourName { get; set; } = string.Empty;
        public decimal PricePaid { get; set; }
        public DateTime PurchasedAt { get; set; }
    }
}

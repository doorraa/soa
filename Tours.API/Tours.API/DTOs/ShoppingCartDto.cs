namespace Tours.API.DTOs
{
    public class OrderItemDto
    {
        public string TourId { get; set; } = string.Empty;
        public string TourName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class ShoppingCartDto
    {
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public decimal TotalPrice { get; set; }
    }
}

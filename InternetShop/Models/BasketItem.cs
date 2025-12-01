
namespace InternetShop.Models
{
    public class BasketItem
    {
        public int Id { get; set; }
        public required string CustomerId { get; set; }
        public required int ProductId { get; set; }
        public required int Quantity { get; set; }

        public Product? Product { get; set; }
    }
}
// Models/Order.cs
using System.ComponentModel.DataAnnotations;

namespace InternetShop.Models
{
    public class Order
    {
        public int Id { get; set; }

        public required string UserId { get; set; }

        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? DeliveryMethod { get; set; }
        public string? DeliveryAddress { get; set; }
        public decimal TotalAmount { get; set; } = 0m;

        public List<OrderItem> Items { get; set; } = new();

        public User? User { get; set; }
    }
}
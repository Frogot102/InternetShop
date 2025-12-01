using InternetShop.Data;
using InternetShop.Models;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.Services
{
    public class OrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderFromBasketAsync(string userId, List<int> basketItemIdsToOrder)
        {
            var basketItems = await _context.BasketItems
                .Where(b => b.CustomerId == userId && basketItemIdsToOrder.Contains(b.Id))
                .Include(b => b.Product)
                .ToListAsync();

            if (!basketItems.Any())
                throw new InvalidOperationException("No items selected for order.");


            foreach (var item in basketItems)
            {
                if (item.Product == null)
                    throw new InvalidOperationException($"Product with ID {item.ProductId} not found.");

                if (!item.Product.IsActive)
                    throw new InvalidOperationException($"Product '{item.Product.Name}' is not available.");

                if (item.Product.Stock < item.Quantity)
                    throw new InvalidOperationException(
                        $"Not enough stock for product '{item.Product.Name}'. " +
                        $"Available: {item.Product.Stock}, requested: {item.Quantity}.");
            }


            var order = new Order
            {
                UserId = userId,
                Items = basketItems.Select(b => new OrderItem
                {
                    ProductId = b.ProductId,
                    Quantity = b.Quantity,
                    PriceAtOrder = b.Product.Price
                }).ToList()
            };


            foreach (var basketItem in basketItems)
            {
                basketItem.Product.Stock -= basketItem.Quantity;

            }

            _context.Orders.Add(order);
            _context.BasketItems.RemoveRange(basketItems);

            await _context.SaveChangesAsync();

            return order;
        }
    }
}

using System.Security.Claims;
using InternetShop.Data;
using InternetShop.Models;
using InternetShop.Requests;
using InternetShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly OrderService _orderService;

        public CustomerController(AppDbContext db, OrderService orderService)
        {
            _db = db;
            _orderService = orderService;
        }

        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest req)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var order = new Order
            {
                UserId = userId,
                DeliveryMethod = req.DeliveryMethod,
                DeliveryAddress = req.DeliveryAddress,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return Ok(order);
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";

            var orders = await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpDelete("orders/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";

            var order = await _db.Orders
                .Where(o => o.Id == orderId && o.UserId == userId)
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
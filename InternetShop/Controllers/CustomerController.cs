using System.Security.Claims;
using InternetShop.Attributes;
using InternetShop.Data;
using InternetShop.Models;
using InternetShop.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly AppDbContext _db;

    public CustomerController(AppDbContext db)
    {
        _db = db;
    }

    [RoleAuthorize("customer")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _db.Users
            .Include(u => u.Addresses)
            .Include(u => u.PaymentMethods)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound();
        return Ok(user);
    }

    [RoleAuthorize("customer")]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.FullName = req.FullName;
        user.Phone = req.Phone;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    [RoleAuthorize("customer")]
    [HttpPost("addresses")]
    public async Task<IActionResult> AddAddress([FromBody] AddressRequest req)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var address = new Address
        {
            UserId = userId,
            Street = req.Street,
            City = req.City,
            PostalCode = req.PostalCode,
            Country = req.Country
        };
        _db.Addresses.Add(address);
        await _db.SaveChangesAsync();
        return Ok(address);
    }

    [RoleAuthorize("customer")]
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest req)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
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

        decimal total = 0;
        foreach (var item in req.Items)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            if (product == null || product.Stock < item.Quantity)
                return BadRequest("Недостаточно товара на складе");

            total += product.Price * item.Quantity;
            product.Stock -= item.Quantity;

            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                PriceAtOrder = product.Price
            });
        }

        order.TotalAmount = total;
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(order);
    }

    [RoleAuthorize("customer")]
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var orders = await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .ToListAsync();
        return Ok(orders);
    }

    [RoleAuthorize("customer")]
    [HttpDelete("orders/{orderId}")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null || order.UserId != userId) return NotFound();

        if (order.Status != "pending")
            return BadRequest("Можно отменить только заказы со статусом 'pending'");

        order.Status = "cancelled";
        await _db.SaveChangesAsync();
        return Ok();
    }
}
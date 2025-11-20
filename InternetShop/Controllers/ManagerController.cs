using System.Security.Claims;
using InternetShop.Attributes;
using InternetShop.Data;
using InternetShop.Models;
using InternetShop.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.Controllers;

[ApiController]
[Route("api/manager")]
public class ManagerController : ControllerBase
{
    private readonly AppDbContext _db;

    public ManagerController(AppDbContext db)
    {
        _db = db;
    }

    [RoleAuthorize("manager", "admin")]
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers()
    {
        var customers = await _db.Users.Where(u => u.Role == "customer").ToListAsync();
        return Ok(customers);
    }

    [RoleAuthorize("manager", "admin")]
    [HttpPut("customers/{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null || user.Role != "customer") return NotFound();

        user.FullName = req.FullName;
        user.Email = req.Email;
        user.Phone = req.Phone;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    [RoleAuthorize("manager", "admin")]
    [HttpDelete("customers/{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null || user.Role != "customer") return NotFound();
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [RoleAuthorize("manager", "admin")]
    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest req)
    {
        if (req.CategoryId.HasValue)
        {
            var category = await _db.Categories.FindAsync(req.CategoryId.Value);
            if (category == null) return BadRequest("Категория не найдена");
        }

        var product = new Product
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            Stock = req.Stock,
            CategoryId = req.CategoryId,
            ImagePath = req.ImagePath,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return Ok(product);
    }

    [RoleAuthorize("manager", "admin")]
    [HttpPut("products/{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] CreateProductRequest req)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.Name = req.Name;
        product.Description = req.Description;
        product.Price = req.Price;
        product.Stock = req.Stock;
        product.CategoryId = req.CategoryId;
        product.ImagePath = req.ImagePath;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(product);
    }

    [RoleAuthorize("manager", "admin")]
    [HttpDelete("products/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [RoleAuthorize("manager", "admin")]
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _db.Orders.Include(o => o.Items).ToListAsync();
        return Ok(orders);
    }

    [RoleAuthorize("manager", "admin")]
    [HttpPut("orders/{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest req)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        var validStatuses = new[] { "pending", "confirmed", "shipped", "delivered", "cancelled" };
        if (!validStatuses.Contains(req.Status))
            return BadRequest("Неверный статус");

        order.Status = req.Status;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(order);
    }

    [RoleAuthorize("manager", "admin")]
    [HttpGet("reports/sales")]
    public async Task<IActionResult> GetSalesReport([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var now = DateTime.UtcNow;
        var startDate = start ?? now.AddDays(-7);
        var endDate = end ?? now;

        var totalRevenue = await _db.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .SumAsync(o => o.TotalAmount);

        var topProducts = await _db.OrderItems
            .Where(oi => _db.Orders.Any(o => o.Id == oi.OrderId && o.CreatedAt >= startDate && o.CreatedAt <= endDate))
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalSold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(10)
            .ToListAsync();

        return Ok(new
        {
            TotalRevenue = totalRevenue,
            TopProducts = topProducts,
            PeriodStart = startDate,
            PeriodEnd = endDate
        });
    }
}

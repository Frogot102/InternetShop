using System.Security.Claims;
using BCrypt.Net;
using InternetShop.Attributes;
using InternetShop.Data;
using InternetShop.Models;
using InternetShop.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }



    [RoleAuthorize("admin")]
    [HttpGet("staff")]
    public async Task<IActionResult> GetAllStaff()
    {
        var staff = await _db.Users
            .Where(u => u.Role == "admin" || u.Role == "manager")
            .ToListAsync();
        return Ok(staff);
    }

    [RoleAuthorize("admin")]
    [HttpPost("staff")]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return BadRequest("Email уже существует");

        if (req.Role != "admin" && req.Role != "manager")
            return BadRequest("Роль должна быть 'admin' или 'manager'");

        var user = new User
        {
            Email = req.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(req.Password),
            FullName = req.FullName,
            Role = req.Role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    [RoleAuthorize("admin")]
    [HttpPut("staff/{id}")]
    public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null || (user.Role != "admin" && user.Role != "manager"))
            return NotFound("Сотрудник не найден");

        user.FullName = req.FullName;
        user.Email = req.Email;
        user.Phone = req.Phone;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    [RoleAuthorize("admin")]
    [HttpDelete("staff/{id}")]
    public async Task<IActionResult> DeleteStaff(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null || (user.Role != "admin" && user.Role != "manager"))
            return NotFound("Сотрудник не найден");

        if (user.Id == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"))
            return BadRequest("Нельзя удалить самого себя");

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return Ok();
    }



    [RoleAuthorize("admin")]
    [HttpGet("customers")]
    public async Task<IActionResult> GetAllCustomers()
    {
        var customers = await _db.Users
            .Where(u => u.Role == "customer")
            .ToListAsync();
        return Ok(customers);
    }

    [RoleAuthorize("admin")]
    [HttpPut("customers/{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null || user.Role != "customer")
            return NotFound("Покупатель не найден");

        user.FullName = req.FullName;
        user.Email = req.Email;
        user.Phone = req.Phone;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    [RoleAuthorize("admin")]
    [HttpDelete("customers/{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null || user.Role != "customer")
            return NotFound("Покупатель не найден");

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

    [RoleAuthorize("admin")]
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

    [RoleAuthorize("admin")]
    [HttpDelete("products/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return Ok();
    }



    [RoleAuthorize("admin")]
    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .ToListAsync();
        return Ok(orders);
    }

    [RoleAuthorize("admin")]
    [HttpPut("orders/{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest req)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        var validStatuses = new[] { "pending", "confirmed", "shipped", "delivered", "cancelled" };
        if (!validStatuses.Contains(req.Status))
            return BadRequest("Неверный статус заказа");

        order.Status = req.Status;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(order);
    }



    [RoleAuthorize("admin")]
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        var validRoles = new[] { "admin", "manager", "customer" };
        if (!validRoles.Contains(req.Role))
            return BadRequest("Недопустимая роль");

        user.Role = req.Role;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(user);
    }



    [RoleAuthorize("admin")]
    [HttpGet("logs")]
    public IActionResult GetLogs()
    {
        return Ok(new[]
        {
            new { Time = DateTime.UtcNow, User = "admin", Action = "Login" },
            new { Time = DateTime.UtcNow.AddMinutes(-5), User = "manager1", Action = "Updated product" }
        });
    }



    [RoleAuthorize("admin")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [RoleAuthorize("admin")]
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
}
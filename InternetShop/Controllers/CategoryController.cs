using InternetShop.Attributes;
using InternetShop.Data;
using InternetShop.Models;
using InternetShop.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoryController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _db.Categories.ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    [RoleAuthorize("admin", "manager")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest req)
    {
        if (req.ParentId.HasValue)
        {
            var parent = await _db.Categories.FindAsync(req.ParentId.Value);
            if (parent == null) return BadRequest("Родительская категория не найдена");
        }

        var category = new Category
        {
            Name = req.Name,
            ParentId = req.ParentId
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return Ok(category);
    }

    [HttpPut("{id}")]
    [RoleAuthorize("admin", "manager")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest req)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();

        if (req.ParentId.HasValue)
        {
            var parent = await _db.Categories.FindAsync(req.ParentId.Value);
            if (parent == null) return BadRequest("Родительская категория не найдена");
        }

        category.Name = req.Name;
        category.ParentId = req.ParentId;
        await _db.SaveChangesAsync();
        return Ok(category);
    }

    [HttpDelete("{id}")]
    [RoleAuthorize("admin", "manager")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();

        var hasChildren = await _db.Categories.AnyAsync(c => c.ParentId == id);
        var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id);

        if (hasChildren || hasProducts)
            return BadRequest("Нельзя удалить категорию, если у неё есть подкатегории или товары");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return Ok();
    }
}
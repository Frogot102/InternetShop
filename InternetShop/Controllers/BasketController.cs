using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using InternetShop.Data;
using InternetShop.Models;
using InternetShop.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ← только авторизованные
    public class BasketController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BasketController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToBasket([FromBody] AddToBasketRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var item = new BasketItem
            {
                CustomerId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            _context.BasketItems.Add(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetBasketItems()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var items = await _context.BasketItems
                .Where(b => b.CustomerId == userId)
                .Include(b => b.Product)
                .ToListAsync();

            return Ok(items);
        }
    }
}

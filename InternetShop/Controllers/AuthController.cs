using BCrypt.Net;
using InternetShop.Data;
using InternetShop.Models;
using InternetShop.Requests;
using InternetShop.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

    public AuthController(AppDbContext db, Microsoft.Extensions.Configuration.IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return BadRequest("Email уже существует");

        var user = new User
        {
            Email = req.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(req.Password),
            FullName = req.FullName,
            Role = "customer"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new Session
        {
            Token = JwtGenerator.GenerateToken(user, _config),
            Role = user.Role
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.Password))
            return Unauthorized();

        return Ok(new Session
        {
            Token = JwtGenerator.GenerateToken(user, _config),
            Role = user.Role
        });
    }
}
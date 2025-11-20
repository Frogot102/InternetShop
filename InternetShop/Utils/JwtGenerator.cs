using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InternetShop.Models;
using Microsoft.IdentityModel.Tokens;

namespace InternetShop.Utils;

public static class JwtGenerator
{
    public static string GenerateToken(User user, Microsoft.Extensions.Configuration.IConfiguration config)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
            new Claim(ClaimTypes.Email, user.Email),                 
            new Claim(ClaimTypes.Role, user.Role)                
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
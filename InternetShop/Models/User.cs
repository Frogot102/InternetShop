using System.ComponentModel.DataAnnotations;
using System.Net;

namespace InternetShop.Models;

public class User
{
    public int Id { get; set; }
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = "customer";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<Address> Addresses { get; set; } = new();
    public List<PaymentMethod> PaymentMethods { get; set; } = new();
}
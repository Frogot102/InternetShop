namespace InternetShop.Models;

public class PaymentMethod
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty; 
    public string Details { get; set; } = string.Empty; 
}
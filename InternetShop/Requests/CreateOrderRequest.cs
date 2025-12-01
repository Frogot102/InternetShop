
namespace InternetShop.Requests
{
    public class CreateOrderRequest
    {
        public string DeliveryMethod { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
    }
}
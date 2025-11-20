namespace InternetShop.Requests;

public class CreateOrderRequest
{
    public string DeliveryMethod { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
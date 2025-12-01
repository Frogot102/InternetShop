
using System.Collections.Generic;

namespace InternetShop.Requests
{
    public class CreateOrderFromBasketRequest
    {
        public List<int> BasketItemIds { get; set; } = new();
    }
}
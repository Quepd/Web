using Web.Models;

namespace Web.ViewModels
{
    public class CartViewModels
    {
        public List<CartItem> cartItems { get; set; } = new List<CartItem>();
    }
}

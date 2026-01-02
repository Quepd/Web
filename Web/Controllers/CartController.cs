using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models;
using Web.ViewModels;

namespace Web.Controllers
{
    [OnlyCustomer]

    public class CartController : Controller
	{
		public IActionResult Index()
		{
            ShoppingDbContext context = new ShoppingDbContext();

            int customerId = Int32.Parse(HttpContext.Session.GetString("UserId") ?? "0");

            if (customerId == 0)
            {
                HttpContext.Session.Remove("UserId");
                return RedirectToAction("Index", "Login");
            }

            CartViewModels viewModels = new CartViewModels();

            viewModels.cartItems = context.CartItems
                .Include(ci => ci.Product)
                .ThenInclude(ci => ci.ProductImage)
                .Where(ci => ci.Cart.CustomerId == customerId)
                .ToList();

            return View(viewModels);
		}
		

        [HttpPost]
        public IActionResult UpdateItem(int productId, bool increase)
        {
			ShoppingDbContext context = new ShoppingDbContext();
			int customerId = Int32.Parse(HttpContext.Session.GetString("UserId") ?? "0");

			if (customerId == 0)
			{
				HttpContext.Session.Remove("UserId");
				return RedirectToAction("Index", "Login");
			}

			var cart = context.Carts.FirstOrDefault(c => c.CustomerId == customerId);

			if (cart == null)
			{
				return NotFound();
			}

			var cartItem = context.CartItems
				.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);

			if (cartItem == null)
			{
				return NotFound();
			}

			if (increase == true)
			{
				cartItem.Quantity++;
			} else
			{				
				cartItem.Quantity--;
			}			

			context.CartItems.Update(cartItem);
			context.SaveChanges();

			return RedirectToAction("Index");
		}

		[HttpPost]
        public IActionResult DeleteItem(int productId)
        {
            ShoppingDbContext context = new ShoppingDbContext();
			int customerId = Int32.Parse(HttpContext.Session.GetString("UserId") ?? "0");

			if (customerId == 0)
			{
				HttpContext.Session.Remove("UserId");
				return RedirectToAction("Index", "Login");
			}

			var cart = context.Carts.FirstOrDefault(c => c.CustomerId == customerId);

			if (cart == null)
			{
				return NotFound();
			}

			var cartItem = context.CartItems
				.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);

			if (cartItem == null)
			{
				return NotFound();
			}

			context.CartItems.Remove(cartItem);
			context.SaveChanges();

			return RedirectToAction("Index");
		}
		[HttpPost]
		public IActionResult Add(int productId)
		{
            ShoppingDbContext context = new ShoppingDbContext();
            var product = context.Products.Find(productId);
            if (product == null)
            {
                return NotFound();
            }

			int customerId = Int32.Parse(HttpContext.Session.GetString("UserId") ?? "0");

			if (customerId == 0)
			{
				HttpContext.Session.Remove("UserId");
                return RedirectToAction("Index", "Login");
            }

            var cart = context.Carts.FirstOrDefault(c => c.CustomerId == customerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,                    
                };
                context.Carts.Add(cart);
                context.SaveChanges();
            }

            context = new ShoppingDbContext();

            var cartItem = context.CartItems
                .FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);

            if (cartItem == null)
            {
                cartItem = new CartItem();
                cartItem.ProductId = productId;
                cartItem.CartId = cart.Id;
                cartItem.Quantity = 1;
                cartItem.Price = product.Price;
                cartItem.Discount = (double)((product.Discount == null) ? 0 : product.Discount);
            }
            else
            {
                cartItem.Quantity++;                
            }

            context.CartItems.Update(cartItem);
            context.SaveChanges();

            return RedirectToAction("Index");   
        }

		public IActionResult Confirmation()
		{
			ShoppingDbContext _context = new ShoppingDbContext();
			int userId = Int32.Parse(HttpContext.Session.GetString("UserId") ?? "0");
			if (userId == null)
			{
				return RedirectToAction("Index", "Login");
			}

			var cart = _context.Carts
				.Include(c => c.CartItems)
				.ThenInclude(ci => ci.Product)
				.FirstOrDefault(c => c.CustomerId == userId);

			if (cart == null || !cart.CartItems.Any())
			{
				return RedirectToAction("Index", "Cart");
			}

			ViewBag.PaymentMethods = new List<string> { "Thanh toán khi nhận hàng", "Chuyển khoản ngân hàng" };
			ViewBag.Addresses = _context.Customers
				.Where(c => c.Id == userId)
				.Select(c => c.Address)
				.ToList();

			return View(cart);
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmOrder(string selectedAddress, string paymentMethod, string newAddress)
        {
            ShoppingDbContext _context = new ShoppingDbContext();
            int userId = Int32.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            if (userId == 0) return RedirectToAction("Index", "Login");

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.CustomerId == userId);

            if (cart == null || !cart.CartItems.Any()) return NotFound();

           
            var shippingAddress = !string.IsNullOrWhiteSpace(newAddress) ? newAddress : selectedAddress;

            var total = cart.CartItems.Sum(i => i.Price * i.Quantity);
            var order = new Order
            {
                CustomerId = userId,
                Status = 0, // Chờ xử lý
                Shipping = 0,
                Discount = 0,
                Total = total,
                GrandTotal = total,
                Address = shippingAddress, 
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Price,
                    Discount = ci.Discount
                }).ToList()
            };

            _context.Orders.Add(order);

            // Xóa giỏ hàng
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Đơn hàng đã được tạo thành công!";
            return RedirectToAction("Index", "Home");
        }

    }
}

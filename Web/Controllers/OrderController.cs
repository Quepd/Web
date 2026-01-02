using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly ShoppingDbContext _context;

        public OrderController(ShoppingDbContext context)
        {
            _context = context;
        }

		public IActionResult Index()
		{
			int userId = Int32.Parse(HttpContext.Session.GetString("UserId") ?? "0");

			if (userId ==0)
			{

				HttpContext.Session.Remove("UserId");
				return RedirectToAction("Index", "Login");
			}

			
			var orders = _context.Orders
				.Include(o => o.Customer)
				.Where(o => o.Customer.Id == userId)
				.ToList();

			return View(orders);
		}


		public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}

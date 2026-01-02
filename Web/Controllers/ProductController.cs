using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Controllers
{
    [OnlyCustomer]

    public class ProductController : Controller
	{
		private readonly ShoppingDbContext _context;

		public ProductController(ShoppingDbContext context)
		{
			_context = context;
		}
		public IActionResult Detail(string id)
		{
			var product = _context.Products
				.Include(p => p.ProductImage)
				.Include(p => p.ProductReviews)
				.FirstOrDefault(p => p.ProductId == id);

			if (product == null)
			{
				return NotFound();
			}

			return View(product);
		}


		public IActionResult Index()
		{
			return View();
		}
	}
}

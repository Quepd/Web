using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models;
using Web.ViewModels;
namespace Web.Controllers
{
	public class ListController : Controller
	{
		private readonly ShoppingDbContext _context;
		public ListController(ShoppingDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(decimal? lowprice, decimal? highprice, int page = 1)
		{
			int pageSize = 6;

			var products = _context.Products
				.Include(p => p.ProductImage)
				.Where(p => p.Active == true);

			if (lowprice.HasValue)
				products = products.Where(p => p.Price >= (double)lowprice.Value);

			if (highprice.HasValue)
				products = products.Where(p => p.Price <= (double)highprice.Value);

			var pagedResult = await PaginatedList<Product>.CreateAsync(products, page, pageSize);

			return View(pagedResult);
		}

        [Route("List/Category/{id:int}")]
        public async Task<IActionResult> Category(int id, decimal? lowprice, decimal? highprice, int page = 1)
		{
			int pageSize = 6;

			var category = await _context.Categories
				.FirstOrDefaultAsync(c => c.Id == id && c.Active == true);
			if (category == null) return NotFound();

			var productIds = _context.ProductCategories
				.Where(pc => pc.CategoryId == id)
				.Select(pc => pc.ProductId);

			var products = _context.Products
				.Include(p => p.ProductImage)
				.Where(p => p.Active == true && productIds.Contains(p.Id));

			if (lowprice.HasValue)
				products = products.Where(p => p.Price >= (double)lowprice.Value);

			if (highprice.HasValue)
				products = products.Where(p => p.Price <= (double)highprice.Value);

			ViewBag.CategoryName = category.Title;
			ViewBag.CategoryId = id;

			var pagedResult = await PaginatedList<Product>.CreateAsync(products, page, pageSize);
			return View("Index", pagedResult);
		}


	}
}

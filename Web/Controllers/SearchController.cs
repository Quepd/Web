using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models;
using Web.ViewModels;
using System.Linq;

namespace Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly ShoppingDbContext _context;

        public SearchController(ShoppingDbContext context)
        {
            _context = context;
        }

		public async Task<IActionResult> Index(string keyword, int? lowprice, int? highprice, int page = 1)
		{
			if (string.IsNullOrWhiteSpace(keyword))
			{
				return RedirectToAction("Index", "Home");
			}

			int pageSize = 6;

			var query = _context.Products
				.Include(p => p.ProductImage)
				.Where(p => p.Active == true &&
							(p.Title.Contains(keyword) || p.Content.Contains(keyword)));

			// Thêm điều kiện lọc theo khoảng giá
			if (lowprice.HasValue)
			{
				query = query.Where(p => p.Price >= lowprice.Value);
			}

			if (highprice.HasValue)
			{
				query = query.Where(p => p.Price <= highprice.Value);
			}

			var result = await PaginatedList<Product>.CreateAsync(query.AsNoTracking(), page, pageSize);

			// Truyền lại để view có thể hiển thị đúng giá trị và phân trang
			ViewBag.Keyword = keyword;
			ViewBag.LowPrice = lowprice;
			ViewBag.HighPrice = highprice;

			return View(result);
		}

	}
}

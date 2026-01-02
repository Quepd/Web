using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models;

[OnlyCustomer]
public class HomeController : Controller

{

	private readonly ShoppingDbContext _context;

	public HomeController(ShoppingDbContext context)
	{
		_context = context;
	}

	public async Task<IActionResult> Index()
	{
		var categories = await _context.Categories.Where(c => c.Active == true).ToListAsync();

		
		var hotDeals = await _context.Products
			.Where(p => p.Active == true && p.Discount != null && p.Discount >= 0)
			.OrderBy(r => Guid.NewGuid())
			.Take(6)
			.Include(p => p.ProductImage)
			.ToListAsync();

		
		var newProducts = await _context.Products
			.Where(p => p.Active == true)
			.OrderByDescending(p => p.Id)
			.Take(20) 
			.OrderBy(p => Guid.NewGuid())
			.Take(6)
			.Include(p => p.ProductImage)
			.ToListAsync();

		ViewBag.HotDeals = hotDeals;
		ViewBag.NewProducts = newProducts;
		return View(categories);
	}
    public IActionResult AccessDenied()
    {
        return View();
    }

}


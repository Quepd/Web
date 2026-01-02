using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Web.Models;
using Web.ViewModels;

namespace Web.Controllers
{ 
	public class BlogController : Controller
	{
		public IActionResult Index()
		{
			ShoppingDbContext context = new ShoppingDbContext();
			BlogViewModels viewModels = new BlogViewModels();

			viewModels.Blogs = context.Blogs.ToList();
			return View(viewModels);
		}
		public IActionResult Detail(int id)
		{
			ShoppingDbContext context = new ShoppingDbContext();
			BlogDetailsViewModels viewModels = new BlogDetailsViewModels();

			viewModels.Blog = context.Blogs.Where(bl => bl.Id == id).FirstOrDefault();

			if (viewModels.Blog == null)
			{
				return RedirectToAction("Index");
			}

			return View(viewModels);
		}
	}
}

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models;
using Web.ViewModels;

namespace Web.Controllers
{
	[OnlyAdmin]
	public class AdminController : Controller
	{
		public IConfiguration Configuration { get; }
		private Cloudinary cloudinary;
		public AdminController(IConfiguration configuration)
		{
			Configuration = configuration;
			cloudinary = new Cloudinary(new Account(
				"dzrpsjtid",
				"825763597552199",
				"gsLEq3IFI_RYWkjm8Dd63g5jPao"
			));
		}


		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}
		[HttpGet]
		public IActionResult Blogs()
		{
			ShoppingDbContext context = new ShoppingDbContext();
			AdminBlogsViewModels viewModels = new AdminBlogsViewModels();
			viewModels.Blogs = context.Blogs.ToList();
			return View(viewModels);
		}
		[HttpGet]
		public IActionResult BlogDetails(int id)
		{
			ShoppingDbContext context = new ShoppingDbContext();
			AdminBlogDetailsViewModels viewModels = new AdminBlogDetailsViewModels();
			viewModels.Blog = context.Blogs.Where(b => b.Id == id).FirstOrDefault();

			if (viewModels.Blog == null)
			{
				return View("Index");
			}

			return View(viewModels);
		}
		public IActionResult AddBlog()
		{
			return View();
		}
		[HttpPost]
		public IActionResult AddBlog(string title, string content)
		{
			ShoppingDbContext context = new ShoppingDbContext();
			Blog blog = new Blog { Title = title, Content = content, CreatedAt = DateTime.UtcNow };

			context.Blogs.Add(blog);
			context.SaveChanges();

			return RedirectToAction("Blogs");
		}
		public IActionResult Customers()
		{
			ShoppingDbContext context = new ShoppingDbContext();
			List<Customer> customers = new List<Customer>();
			customers = context.Customers
				.Include(c => c.IdNavigation) // Include User details if needed
				.ToList();
			AdminCustomerViewModels viewModels = new AdminCustomerViewModels();
			viewModels.Customers = customers;

			return View(viewModels);
		}

		public IActionResult AddProduct()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> AddProduct(string title, double price, double discount, string content, int category, IFormFile image)
		{
			if (image.Length == 0 || string.IsNullOrEmpty(title) || price <= 0 || category <= 0 || string.IsNullOrEmpty(content))
			{
				return RedirectToAction("Products");
			}

			ShoppingDbContext context = new ShoppingDbContext();

			Product product = new Product
			{
				ProductId = context.Products.Count().ToString(),
				Title = title,
				Price = price,
				Discount = discount,
				Content = content,
				Active = true,
				Type = 1
			};

			context.Products.Add(product);
			context.SaveChanges();

			context = new ShoppingDbContext();

			ProductCategory productCategory = new ProductCategory
			{
				ProductId = product.Id,
				CategoryId = category,
				Category = context.Categories
					.Where(c => c.Id == category)
					.FirstOrDefault(),
				Product = context.Products
					.Where(p => p.Title == product.Title)
					.FirstOrDefault()
			};

			context.ProductCategories.Add(productCategory);
			context.SaveChanges();
			context = new ShoppingDbContext();

			var uploadResult = new ImageUploadResult();			

			using (var stream = image.OpenReadStream())
			{
				var uploadParams = new ImageUploadParams()
				{
					File = new FileDescription(image.FileName, stream)
				};

				uploadResult = await cloudinary.UploadAsync(uploadParams);
			}

			string filePath = uploadResult.Uri.ToString();

			ProductImage productImage = new ProductImage
			{
				ProductId = product.Id,
				ImageSrc = filePath
			};
			
			context.ProductImages.Add(productImage);
			context.SaveChanges();

			return RedirectToAction("Products");
		}
		[HttpGet]
		public IActionResult Products()
		{
			ShoppingDbContext context = new ShoppingDbContext();
			List<Product> products = new List<Product>();
			products = context.Products.ToList();

			AdminProductsViewModels viewModels = new AdminProductsViewModels();
			viewModels.Products = products;
			return View(viewModels);
		}
		[HttpGet]
		public IActionResult ProductDetails(int id)
		{
			ShoppingDbContext context = new ShoppingDbContext();
			AdminProductDetailsViewModels viewModels = new AdminProductDetailsViewModels();
			viewModels.Product = context.Products
				.Where(p => p.Id == id)
				.FirstOrDefault();

			if (viewModels.Product == null)
			{
				return NotFound();
			}

			return View(viewModels);
		}
		[HttpPost]
		public IActionResult EditProduct(int id, string title, int price, int discount, bool active)
		{
			ShoppingDbContext context = new ShoppingDbContext();
			Product? product = context.Products
				.Where(p => p.Id == id)
				.FirstOrDefault();

			product.Title = title;
			product.Price = price;
			product.Discount = discount;
			product.Active = active;

			context.Update(product);
			context.SaveChanges();

			return RedirectToAction("Products");
		}



        public IActionResult AccessDenied()
        {
            return View();
        }

		[HttpPost]
		public IActionResult Logout()
		{

			HttpContext.Session.Clear();

			HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			return RedirectToAction("Index", "Home");
		}
	}
}

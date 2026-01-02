using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Controllers
{
    [OnlyCustomer]

    public class ProfileController : Controller
	{
		private readonly ShoppingDbContext _context;

		public ProfileController(ShoppingDbContext context)
		{
			_context = context;
		}

		// GET: /Profile
		public IActionResult Index()
		{
			var userIdStr = HttpContext.Session.GetString("UserId");
			if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
			{
				return RedirectToAction("Index", "Login");
			}

			var customer = _context.Customers
				.Include(c => c.IdNavigation)
				.FirstOrDefault(c => c.Id == userId);

			if (customer == null)
			{
				return NotFound();
			}

			return View(customer);
		}

		// POST: /Profile/Update
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Update(Customer model)
		{
			var userIdStr = HttpContext.Session.GetString("UserId");
			if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
			{
				return RedirectToAction("Index", "Login");
			}

			var customer = _context.Customers.FirstOrDefault(c => c.Id == userId);
			if (customer == null)
			{
				return NotFound();
			}


			bool isChanged =
			customer.FullName != model.FullName ||
			customer.PhoneNumber != model.PhoneNumber ||
			customer.DateofBirth != model.DateofBirth ||
			customer.Address != model.Address;

			if (!isChanged)
			{
				TempData["SuccessMessage"] = "Không có thay đổi nào để cập nhật.";
				return RedirectToAction("Index");
			}


			customer.FullName = model.FullName;
			customer.PhoneNumber = model.PhoneNumber;
			customer.DateofBirth = model.DateofBirth;
			customer.Address = model.Address;


			_context.SaveChanges();

			TempData["SuccessMessage"] = "Cập nhật thành công!";
			return RedirectToAction("Index");
		}



	}
}

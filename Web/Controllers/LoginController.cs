using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Web.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;

public class LoginController : Controller
{
    private readonly ShoppingDbContext _context;

    public LoginController(ShoppingDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
	[HttpPost]
	public IActionResult Index(string identifier, string password)
	{
		if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(password))
		{
			ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
			return View();
		}

		string passwordHash = GetSha256Hash(password);

		var user = _context.Users
			.Include(u => u.Customer)
			.Include(u => u.Admin)
			.FirstOrDefault(u =>
				(
					(u.Admin != null && u.UserName == identifier) || // Admin login bằng username
					(u.Customer != null && (u.Email == identifier || u.Customer.PhoneNumber == identifier)) // Customer login bằng email hoặc sđt
				)
				&& u.PasswordHash == passwordHash
				&& u.Active == true
			);

		if (user == null)
		{
			ViewBag.Error = "Email/SĐT hoặc mật khẩu không đúng.";
			return View();
		}

		// ✅ Lưu session
		HttpContext.Session.SetString("UserId", user.Id.ToString());
		HttpContext.Session.SetString("UserName", user.UserName);

		// ✅ Phân quyền bằng session
		if (user.Admin != null)
		{
			HttpContext.Session.SetString("UserRole", "Admin");
			return RedirectToAction("Index", "Admin");
		}
		else if (user.Customer != null)
		{
			HttpContext.Session.SetString("UserRole", "Customer");
			return RedirectToAction("Index", "Home");
		}

		// Trường hợp không xác định vai trò
		ViewBag.Error = "Không xác định được loại người dùng.";
		return View();
	}



	// Bắt đầu đăng nhập với Google
	public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleResponse")
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        var claims = result.Principal?.Identities?.FirstOrDefault()?.Claims;
        string email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        string name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return RedirectToAction("Index", "Home");
        }

        // Tìm user trong hệ thống
        var existingUser = await _context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser == null)
        {
            // Tạo User mới
            var newUser = new User
            {
                UserName = name ?? email.Split('@')[0],
                Password = "123",
                Email = email,
                Active = true
            };

            // Hash mật khẩu giả (do cần trường PasswordHash)
            var passwordHasher = new PasswordHasher<User>();
            newUser.PasswordHash = passwordHasher.HashPassword(newUser, "DefaultPasswordForGoogleLogin");

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync(); 

            // Tạo Customer gắn với User
            var newCustomer = new Customer
            {
                Id = newUser.Id,                 
                FullName = newUser.UserName,
                Gender = null,
                PhoneNumber = null,
                DateofBirth = null,
                Address = null
            };

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync(); // Lưu Customer

            existingUser = newUser; // Gán lại để dùng dưới
        }

        // Lưu thông tin đăng nhập vào session
        HttpContext.Session.SetString("UserId", existingUser.Id.ToString());
        HttpContext.Session.SetString("UserName", existingUser.UserName);

        return RedirectToAction("Index", "Home");
    }



	[HttpPost]
	public async Task<IActionResult> Register(UserDTO model)
	{
		if (!ModelState.IsValid)
			return View(model);

		// Kiểm tra trùng email hoặc số điện thoại
		if (_context.Users.Any(u => u.Email == model.Email || u.UserName == model.PhoneNumber))
		{
			ModelState.AddModelError("", "Email hoặc số điện thoại đã tồn tại.");
			return View(model);
		}

		// Tạo mã OTP ngẫu nhiên
		string otp = new Random().Next(100000, 999999).ToString();

		// Lưu tạm thông tin người dùng và mã OTP vào TempData
		TempData["OTP"] = otp;
		TempData["PendingUser"] = JsonConvert.SerializeObject(model);

		// Gửi OTP qua email trực tiếp
		var smtpClient = new SmtpClient("smtp.gmail.com")
		{
			Port = 587,
			Credentials = new NetworkCredential("phamque2003@gmail.com", "xtjq afbt rskq bibq"),
			EnableSsl = true,
		};

		var mailMessage = new MailMessage
		{
			From = new MailAddress("phamque2003@gmail.com", "Orione Beauty"),
			Subject = "Mã xác thực tài khoản (OTP)",
			Body = $@"
        <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f4f4f4;'>
            <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                <h2 style='color: #2d3436;'>Xin chào {model.UserName},</h2>
                <p>Bạn hoặc ai đó đã sử dụng địa chỉ email này để đăng ký tài khoản tại <strong>Orione Beauty</strong>.</p>
                <p>Để hoàn tất quá trình đăng ký, vui lòng nhập mã OTP bên dưới:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <span style='font-size: 32px; color: #0984e3; font-weight: bold;'>{otp}</span>
                </div>
                <p style='color: #636e72;'>Nếu bạn không thực hiện hành động này, vui lòng bỏ qua email này.</p>
                <p>Trân trọng,<br><strong>Orione Beauty</strong></p>
            </div>
        </div>",
			IsBodyHtml = true
		};


		mailMessage.To.Add(model.Email);

		try
		{
			await smtpClient.SendMailAsync(mailMessage);
		}
		catch (Exception ex)
		{
			ModelState.AddModelError("", "Không thể gửi email: " + ex.Message);
			return View(model);
		}

		return RedirectToAction("VerifyOtp");
	}

	[HttpGet]
	public IActionResult VerifyOtp()
	{
		return View();
	}

	[HttpPost]
	public IActionResult VerifyOtp(string otp)
	{
		string expectedOtp = TempData["OTP"] as string;
		string userJson = TempData["PendingUser"] as string;

		if (otp != expectedOtp || userJson == null)
		{
			ViewBag.Error = "Mã OTP không đúng hoặc đã hết hạn.";
			return View();
		}

		var model = JsonConvert.DeserializeObject<UserDTO>(userJson);

		string passwordHash = GetSha256Hash(model.Password);

		var user = new User
		{
			UserName = model.UserName,
			Password = model.Password,
			PasswordHash = passwordHash,
			Email = model.Email,
			Active = true
		};

		_context.Users.Add(user);
		_context.SaveChanges();

		var customer = new Customer
		{
			Id = user.Id,
			FullName = model.UserName,
			Gender = model.Gender,
			PhoneNumber = model.PhoneNumber,
			DateofBirth = model.DoB,
			Address = null
		};

		_context.Customers.Add(customer);
		_context.SaveChanges();

		return RedirectToAction("Index", "Login");
	}





	private string GetSha256Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }



	[HttpPost]
	public IActionResult Logout()
	{
		
		HttpContext.Session.Clear();

		HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

		return RedirectToAction("Index", "Home");
	}

}

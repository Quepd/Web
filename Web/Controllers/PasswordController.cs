using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Web.Models; 
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.ViewModels;

namespace Web.Controllers
{
	public class PasswordController : Controller
	{
		private readonly ShoppingDbContext _context;

		public PasswordController(ShoppingDbContext context)
		{
			_context = context;
		}

		// --- STEP 1: Hiển thị form nhập email forgot password ---
		[HttpGet]
		public IActionResult ForgotPassword()
		{
			return View();
		}
		[HttpGet]
		public IActionResult ChangePassword()
		{
			return View();
		}

		// --- STEP 2: Xử lý khi người dùng nhập email ---
		[HttpPost]
		public IActionResult ForgotPassword(string email)
		{
			// Tìm user theo email (có thể giới hạn chỉ cho loại user nào nếu cần)
			var user = _context.Users.FirstOrDefault(u => u.Email == email);
			if (user == null)
			{
				ViewBag.Error = "Email không tồn tại.";
				return View();
			}

			// Tạo OTP ngẫu nhiên (6 chữ số)
			var otp = new Random().Next(100000, 999999).ToString();

			// Lưu OTP và email vào Session để dùng cho bước xác thực
			HttpContext.Session.SetString("ResetOTP", otp);
			HttpContext.Session.SetString("ResetEmail", email);

			// Gửi OTP qua email
			try
			{
				var mailMessage = new MailMessage
				{
					From = new MailAddress("phamque2003@gmail.com", "Orione Beauty"),
					Subject = "Mã OTP đặt lại mật khẩu",
					Body = $@"
    <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f8f9fa; border-radius: 8px; max-width: 500px; margin: auto; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
        <h2 style='color: #d63384;'>Orione Beauty</h2>
        <p style='font-size: 16px; color: #333;'>Xin chào,</p>
        <p style='font-size: 16px; color: #333;'>Bạn đã yêu cầu đặt lại mật khẩu. Mã OTP của bạn là:</p>
        <div style='font-size: 28px; color: #0d6efd; font-weight: bold; margin: 16px 0;'>{otp}</div>
        <p style='font-size: 14px; color: #555;'>Vui lòng không chia sẻ mã này với bất kỳ ai để đảm bảo bảo mật tài khoản của bạn.</p>
        <p style='font-size: 14px; color: #555;'>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
        <hr style='margin: 20px 0;' />
        <p style='font-size: 12px; color: #aaa;'>© 2025 Orione Beauty. All rights reserved.</p>
    </div>",
					IsBodyHtml = true
				};


				mailMessage.To.Add(email);

				using (var smtp = new SmtpClient("smtp.gmail.com", 587))
				{
					smtp.Credentials = new NetworkCredential("phamque2003@gmail.com", "xtjq afbt rskq bibq");
					smtp.EnableSsl = true;
					smtp.Send(mailMessage);
				}
			}
			catch (Exception ex)
			{
				ViewBag.Error = "Lỗi gửi email: " + ex.Message;
				return View();
			}

			// Sau khi gửi thành công, chuyển đến trang xác nhận OTP
			return RedirectToAction("VerifyOtp");
		}

		// --- STEP 3: Hiển thị form nhập OTP ---
		[HttpGet]
		public IActionResult VerifyOtp()
		{
			return View();
		}

		// --- STEP 4: Xử lý xác nhận OTP ---
		[HttpPost]
		public IActionResult VerifyOtp(string otp)
		{
			var sessionOtp = HttpContext.Session.GetString("ResetOTP");

			if (otp == sessionOtp)
			{
				return RedirectToAction("ResetPassword");
			}

			ViewBag.Error = "Mã OTP không đúng. Vui lòng thử lại.";
			return View();
		}

		// --- STEP 5: Hiển thị form đặt lại mật khẩu ---
		[HttpGet]
		public IActionResult ResetPassword()
		{
			return View();
		}

		// --- STEP 6: Xử lý đặt lại mật khẩu mới ---
		[HttpPost]
		public IActionResult ResetPassword(string newPassword)
		{
			var email = HttpContext.Session.GetString("ResetEmail");
			var user = _context.Users.FirstOrDefault(u => u.Email == email);

			if (user == null)
			{
				// Nếu user không tìm thấy, chuyển về form forgot password
				return RedirectToAction("ForgotPassword");
			}

			// Cập nhật mật khẩu mới
			user.Password = newPassword;
			user.PasswordHash = GetSha256Hash(newPassword);
			_context.SaveChanges();

			// Xóa thông tin OTP, email khỏi Session sau khi hoàn thành
			HttpContext.Session.Remove("ResetOTP");
			HttpContext.Session.Remove("ResetEmail");

			// Redirect về trang đăng nhập
			return RedirectToAction("Index", "Login");
		}

		


		[HttpPost]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (model.NewPassword != model.ConfirmPassword)
			{
				ViewBag.Error = "Xác nhận mật khẩu không khớp.";
				return View();
			}

			var userId = HttpContext.Session.GetString("UserId");
			if (string.IsNullOrEmpty(userId))
				return RedirectToAction("Index", "Login");

			var user = await _context.Users.FindAsync(int.Parse(userId));
			if (user == null)
			{
				ViewBag.Error = "Không tìm thấy người dùng.";
				return View();
			}

			string hashedOldPassword = GetSha256Hash(model.OldPassword);

			if (user.PasswordHash != hashedOldPassword)
			{
				ViewBag.Error = "Mật khẩu hiện tại không đúng.";
				return View();
			}
			user.Password = model.NewPassword;
			user.PasswordHash = GetSha256Hash(model.NewPassword);
			await _context.SaveChangesAsync();

			return RedirectToAction("Index", "Profile");
		}


		// --- Hàm băm SHA256 cho mật khẩu ---
		private string GetSha256Hash(string input)
		{
			using var sha256 = SHA256.Create();
			var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
			return BitConverter.ToString(bytes).Replace("-", "").ToLower();
		}
	}
}

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Web.Models;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//// config web
//var certPath = Path.Combine(Directory.GetCurrentDirectory(), "certs", "fullchain.pem");
//var keyPath = Path.Combine(Directory.GetCurrentDirectory(), "certs", "privkey_no_pass.pem");

//// Nạp chứng chỉ từ tệp PEM
//var certificate = new X509Certificate2(certPath);

//// Đọc khóa riêng từ tệp PEM và nạp vào đối tượng RSA
//string privateKey = File.ReadAllText(keyPath);
//var rsa = RSA.Create();
//rsa.ImportFromPem(privateKey.ToCharArray());  // Import khóa riêng từ PEM

//// Kết hợp chứng chỉ và khóa riêng
//certificate = certificate.CopyWithPrivateKey(rsa);

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(8080); // HTTP
//    options.ListenAnyIP(8081, listenOptions =>
//    {
//        listenOptions.UseHttps(certificate); // Sử dụng chứng chỉ kết hợp với khóa riêng
//    });
//});
    


builder.Services.AddDbContext<ShoppingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

builder.Services.AddSession(Options => { Options.IdleTimeout = TimeSpan.FromMinutes(45); });

// Cấu hình Authentication với Google
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie() // Sử dụng Cookie Authentication
.AddGoogle(options =>
{
    options.ClientId = "933776615188-m3v3vvevbpe8risegn6mqtu9c2obu880.apps.googleusercontent.com";
    options.ClientSecret = "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";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}
app.UseAuthentication();  // Thêm dòng này để kích hoạt Authentication
app.UseAuthorization();
app.UseSession(); // Đảm bảo gọi UseSession sau UseRouting và trước UseAuthorization


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
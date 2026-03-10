using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; // Thêm thư viện này
using RealEstateWeb.Hubs;
using RealEstateWeb.Models;
using RealEstateWeb.ViewModels;

namespace RealEstateWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IHubContext<NotificationHub> _hubContext; // Khai báo Hub

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IHubContext<NotificationHub> hubContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _hubContext = hubContext; // Khởi tạo Hub
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Dùng Email làm UserName luôn cho tiện
                var user = new AppUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // --- ĐOẠN CODE GỬI THÔNG BÁO SIGNALR ---
                    // Gửi tin nhắn có tên là "ReceiveNotification" tới tất cả các client đang kết nối
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"Có người dùng mới vừa đăng ký: {user.Email}");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(model);
        }

        // GET: /Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Kiểm tra đăng nhập
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // 1. Tìm tài khoản vừa đăng nhập thành công
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    // Ưu tiên 1: Nếu có link cũ đang xem dở (returnUrl) thì trả về link đó
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // Ưu tiên 2: Kiểm tra nếu là Admin thì đẩy thẳng vào trang Quản trị (Dashboard)
                    if (user != null && await _userManager.IsInRoleAsync(user, "Administrator"))
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }

                    // Ưu tiên 3: Nếu là user bình thường thì về Trang chủ
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                    return View(model);
                }
            }
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
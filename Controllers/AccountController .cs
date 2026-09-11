using DATN.Data;
using DATN.Models.Entities;
using DATN.Models.ViewModels;
using DATN.Services.Implementations;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
            
namespace DATN.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context; 

        private readonly IUserService _userService;
        private readonly ISellerService _sellerService;

        public AccountController(AppDbContext context, IUserService userService, ISellerService sellerService)
        {
            _context = context;
            _userService = userService;
            _sellerService = sellerService;
        }
  

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userService.RegisterAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.AuthenticateAsync(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleName ?? "Customer")
            };
            if (user.RoleName == "Seller")
            {
                // Ví dụ: truy vấn tìm ShopId dựa vào UserID của người bán
                var shop = await _context.Shops.FirstOrDefaultAsync(s => s.UserId == user.UserID);

                if (shop != null)
                {
                    claims.Add(new Claim("ShopId", shop.ShopId.ToString()));
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = model.RememberMe });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Redirect based on role
            if (user.RoleName == "Admin")
                return RedirectToAction("Index", "Categories", new { area = "Admin" });

            return RedirectToAction("Index", "Product");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound();
            var sellerProfile = await _context.SellerProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
            ViewBag.SellerStatus = sellerProfile?.Status;
            var viewModel = new ProfileViewModel
            {
                UserID = user.UserID,
                Email = user.Email,
                RoleName = user.RoleName,
                FullName = user.FullName,
                Phone = user.Phone,
            };
            return View(viewModel);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _userService.UpdateProfileAsync(userId, model);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["Success"] = "Cập nhật thông tin thành công.";
            return RedirectToAction(nameof(Profile));
        }

        // GET: /Account/ChangePassword
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["Success"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Profile));
        }
        [HttpGet]
        public IActionResult RegisterSeller()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Authorize] // Nên thêm attribute này để tự động chặn người dùng chưa đăng nhập
        public async Task<IActionResult> RegisterSeller(RegisterSellerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Lấy chuỗi ID từ Claims (thường được lưu dưới dạng NameIdentifier)
            // Nếu lúc đăng nhập bạn dùng custom claim tên "UserId", hãy đổi thành: User.FindFirstValue("UserId")
            string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Kiểm tra và ép kiểu sang int
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                // Nếu dùng [Authorize] ở trên, đoạn redirect này có thể bỏ đi vì hệ thống đã tự lo
                return RedirectToAction("Login", "Account");
            }

            // 3. Truyền userId thẳng vào service (không cần .Value nữa vì biến userId giờ là kiểu int, không phải int?)
            var result = await _sellerService.RegisterSellerAsync(userId, model);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                if (result.Message.Contains("đã gửi yêu cầu"))
                {
                    return RedirectToAction("Profile");
                }
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Profile");
        }
    }
}
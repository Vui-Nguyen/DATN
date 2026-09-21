using DATN.Areas.Seller.Models.ViewModels;
using DATN.Data;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DATN.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "Seller")]
    public class ShopProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IShopService _shopService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShopProfileController(IShopService shopService, IHttpContextAccessor httpContextAccessor, AppDbContext context)
        {
            _shopService = shopService;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(userIdClaim);

            // Lấy shop của user
            var shop = await _context.Shops
                .Where(s => s.UserId == userId)
                .Select(s => s.ShopId)
                .FirstOrDefaultAsync();

            var shopDto = await _shopService.GetShopByIdAsync(shop);
            if (shopDto == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin shop của bạn." });
            }

            return View(shopDto);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateShop(int shopId)
        {
            var shop = await _shopService.GetShopByIdAsync(shopId);
            if (shop == null)
            {
                return NotFound("Không tìm thấy shop.");
            }


            var model = new ShopViewModel
            {
                ShopName = shop.ShopName,
                Description = shop.Description,
                AvatarShop = shop.AvatarShop
            };

           
            ViewBag.ShopId = shopId;
            return View(model);
        }

        // 2. XỬ LÝ LƯU DỮ LIỆU (Dùng POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateShop(int shopId, ShopViewModel model)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.ShopId = shopId;
                return View(model);
            }

            var result = await _shopService.UpdateShopAsync(shopId, model);
            if (!result)
            {
                ModelState.AddModelError("", "Cập nhật thất bại hoặc không tìm thấy shop.");
                ViewBag.ShopId = shopId;
                return View(model);
            }

            TempData["SuccessMessage"] = "Cập nhật thông tin shop thành công!";
            return RedirectToAction("Index");
        }
    }
}

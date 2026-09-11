using DATN.Data;
using DATN.Models.ViewModels;
using DATN.Services.Implementations;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly ISellerService _sellerService;
        private readonly AppDbContext _context;

        public UsersController(IUserService userService, AppDbContext context, ISellerService sellerService)
        {
            _userService = userService;
            _context = context;
            _sellerService = sellerService;
        }

        // GET: /Admin/Users/Index
        public async Task<IActionResult> Index(int page = 1, string? keyword = null)
        {
            var users = await _userService.GetAllAsync(page, keyword);
            ViewBag.Keyword = keyword;
            return View(users);
        }

        // GET: /Admin/Users/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: /Admin/Users/ToggleLock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var result = await _userService.ToggleLockAsync(id);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Users/ChangeRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int id, int roleId)
        {
            var result = await _userService.ChangeRoleAsync(id, roleId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        // 1. Hàm hiển thị danh sách người dùng đang chờ duyệt
        public async Task<IActionResult> PendingSellers()
        {
            var pendingList = await _context.SellerProfiles
                .Include(s => s.User)
                .Where(s => s.Status == 0)
                .ToListAsync();

            return View(pendingList); // Bạn tự tạo một View dạng Table để hiển thị danh sách này nhé
        }

        // 2. Hàm xử lý DUYỆT yêu cầu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveSeller(int id)
        {
            var result = await _sellerService.ApproveSellerAsync(id);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = result.Message;
            }

            return RedirectToAction("PendingSellers");
        }

        [HttpPost]
        public async Task<IActionResult> RejectSeller(int id)
        {
            var profile = await _context.SellerProfiles.FirstOrDefaultAsync(s => s.Id == id);

            if (profile != null)
            {
                profile.Status = 2; 
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã từ chối yêu cầu đăng ký Seller.";
            }
            return RedirectToAction("PendingSellers");
        }
    }
}
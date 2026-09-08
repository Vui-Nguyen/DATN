using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DATN.Services.Interfaces;
using DATN.Models.ViewModels;

namespace DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
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
    }
}
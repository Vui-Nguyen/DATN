using System.Security.Claims;
using DATN.Areas.Admin.Models.ViewModels;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: /Admin/Categories/Index (Danh sách danh mục đã duyệt, có phân trang)
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var pagedCategories = await _categoryService.GetAllPagedAsync(page, pageSize);
            return View(pagedCategories);
        }

        // GET: /Admin/Categories/Pending (Danh sách đề xuất chờ Admin duyệt)
        public async Task<IActionResult> Pending(int page = 1, int pageSize = 10)
        {
            var pendingCategories = await _categoryService.GetPendingApprovalPagedAsync(page, pageSize);
            return View(pendingCategories);
        }

        // POST: /Admin/Categories/Approve/5 (Admin duyệt đề xuất)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _categoryService.ApproveAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = result.Message;
            }

            return RedirectToAction(nameof(Pending));
        }

 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _categoryService.RejectAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = result.Message;
            }

            return RedirectToAction(nameof(Pending));
        }

        // GET: /Admin/Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Admin tạo trực tiếp nên isApproved = true
            var result = await _categoryService.CreateAsync(model, createdByUserId: currentUserId, isApproved: true);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();

            return View(new CategoryViewModel { CategoryName = category.CategoryName });
        }

        // POST: /Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _categoryService.UpdateAsync(id, model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}
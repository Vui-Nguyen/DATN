using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DATN.Services.Interfaces;
using DATN.Areas.Admin.Models.ViewModels;

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

        // GET: /Admin/Categories/Index
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories);
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

            await _categoryService.CreateAsync(model);
            TempData["Success"] = "Tạo danh mục thành công.";
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
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["Success"] = "Cập nhật danh mục thành công.";
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
                TempData["Success"] = "Đã xóa danh mục.";

            return RedirectToAction(nameof(Index));
        }
    }
}
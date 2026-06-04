using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DATN.Services.Interfaces;
using DATN.Models.ViewModels;

namespace DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BrandsController : Controller
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        // GET: /Admin/Brands/Index
        public async Task<IActionResult> Index()
        {
            var brands = await _brandService.GetAllAsync();
            return View(brands);
        }

        // GET: /Admin/Brands/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Brands/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _brandService.CreateAsync(model);
            TempData["Success"] = "Tạo thương hiệu thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Brands/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _brandService.GetByIdAsync(id);
            if (brand == null) return NotFound();

            return View(new BrandViewModel { BrandName = brand.BrandName });
        }

        // POST: /Admin/Brands/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BrandViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _brandService.UpdateAsync(id, model);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["Success"] = "Cập nhật thương hiệu thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Brands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _brandService.DeleteAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = "Đã xóa thương hiệu.";

            return RedirectToAction(nameof(Index));
        }
    }
}
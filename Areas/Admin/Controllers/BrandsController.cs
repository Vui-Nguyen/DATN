using System.Security.Claims;
using DATN.Areas.Admin.Models.ViewModels;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // GET: /Admin/Brands/Index (Danh sách đã duyệt, có phân trang)
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var pagedBrands = await _brandService.GetAllPagedAsync(page, pageSize);
            return View(pagedBrands);
        }

        // GET: /Admin/Brands/Pending (Danh sách chờ phê duyệt từ người bán)
        public async Task<IActionResult> Pending(int page = 1, int pageSize = 10)
        {
            var pendingBrands = await _brandService.GetPendingApprovalPagedAsync(page, pageSize);
            return View(pendingBrands);
        }

        // POST: /Admin/Brands/Approve/5 (Admin duyệt đề xuất)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _brandService.ApproveAsync(id);
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

        // POST: /Admin/Brands/Reject/5 (Admin từ chối đề xuất)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _brandService.RejectAsync(id);
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

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Admin tạo trực tiếp nên isApproved = true
            var result = await _brandService.CreateAsync(model, createdByUserId: currentUserId, isApproved: true);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;
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
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;
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
                TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DATN.Services.Interfaces;
using DATN.Models.ViewModels;

namespace DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IWebHostEnvironment _env;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService,
            IWebHostEnvironment env)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _env = env;
        }

        // GET: /Admin/Products/Index
        public async Task<IActionResult> Index(int page = 1)
        {
            var products = await _productService.GetAllAdminAsync(page);
            return View(products);
        }

        // GET: /Admin/Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetDetailAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // GET: /Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View();
        }

        // POST: /Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model, List<IFormFile>? images)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(model);
            }

            var result = await _productService.CreateAsync(model, images, _env.WebRootPath);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await LoadDropdownsAsync();
                return View(model);
            }

            TempData["Success"] = "Tạo sản phẩm thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetForEditAsync(id);
            if (product == null) return NotFound();

            await LoadDropdownsAsync();
            return View(product);
        }

        // POST: /Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model, List<IFormFile>? images)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(model);
            }

            var result = await _productService.UpdateAsync(id, model, images, _env.WebRootPath);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await LoadDropdownsAsync();
                return View(model);
            }

            TempData["Success"] = "Cập nhật sản phẩm thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = "Đã xóa sản phẩm.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync()
        {
            var categories = await _categoryService.GetAllAsync();
            var brands = await _brandService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName");
            ViewBag.Brands = new SelectList(brands, "BrandID", "BrandName");
        }
    }
}
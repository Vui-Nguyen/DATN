using Microsoft.AspNetCore.Mvc;
using DATN.Services.Interfaces;
using DATN.Models.ViewModels;

namespace DATN.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        // GET: /Product/Index
        public async Task<IActionResult> Index(int page = 1, int pageSize = 12)
        {
            var products = await _productService.GetAllAsync(page, pageSize);
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = categories;
            return View(products);
        }

        // GET: /Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetDetailAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // GET: /Product/Search?keyword=...
        // GET: /Product/Search?keyword=...
       
        public async Task<IActionResult> Search(string keyword, int page = 1, int pageSize = 12)
        {
            if (string.IsNullOrWhiteSpace(keyword)) 
         return RedirectToAction(nameof(Index)); 

     var products = await _productService.SearchAsync(keyword, page, pageSize); 
   ViewBag.Keyword = keyword; 

    // 2. BỔ SUNG: Nạp lại danh sách tất cả danh mục để thanh Sidebar bên cạnh không bị trống dữ liệu
  var categories = await _categoryService.GetAllAsync();
ViewBag.Categories = categories;

    // 3. SỬA ĐỔI: Ép buộc Controller hiển thị kết quả bằng giao diện Index.cshtml
    return View("Index", products);
        }

        // GET: /Product/ProductsByCategory/5
        public async Task<IActionResult> ProductsByCategory(int categoryId, int page = 1, int pageSize = 12)
        {
            var category = await _categoryService.GetByIdAsync(categoryId);
            if (category == null) return NotFound();

            var products = await _productService.GetByCategoryAsync(categoryId, page, pageSize);
            ViewBag.Category = category;
            return View(products);
        }
    }
}
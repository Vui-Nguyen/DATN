using DATN.Models.ViewModels;
using DATN.Services.Implementations;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DATN.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IProductService _productService;

        public ReviewController(IReviewService reviewService, IProductService productService)
        {
            _reviewService = reviewService;
            _productService = productService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        // GET: /Review/Create?variantId=10
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            int userId = GetUserId(); 
            var result = await _reviewService.GetDeliveredProductsByUserIdAsync(userId, page, pageSize);
            return View(result);
        }
        [HttpGet]
        public async Task<IActionResult> Create(int variantId)
        {
            int productId = await _productService.GetProductIdByVariantIdAsync(variantId);

            if (productId == 0) 
            {
                TempData["Error"] = "Không tìm thấy sản phẩm tương ứng.";
                return RedirectToAction("MyOrders", "Order");
            }

            var model = new CreateReviewViewModel
            {
                ProductId = productId, 
            };

            return View(model);
        }
        // POST: /Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu đánh giá không hợp lệ.";
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }

            model.UserId = GetUserId();
            var result = await _reviewService.CreateAsync(model);

            if (!result.Success)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = "Đánh giá của bạn đã được gửi.";

            return RedirectToAction("Index", "Review");
        }

        // GET: /Review/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review == null) return NotFound();

            if (review.UserID != GetUserId())
                return Forbid();
            var model = new EditReviewViewModel
            {
                Rating = review.Rating,
                Comment = review.Comment,
               
            };
            return View(model);
        }

        // POST: /Review/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditReviewViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var review = await _reviewService.GetByIdAsync(id);
            if (review == null) return NotFound();
            if (review.UserID != GetUserId()) return Forbid();

            var result = await _reviewService.UpdateAsync(id, model);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["Success"] = "Cập nhật đánh giá thành công.";
            return RedirectToAction("Index", "Review");
        }

        // POST: /Review/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review == null) return NotFound();
            if (review.UserID != GetUserId()) return Forbid();

            await _reviewService.DeleteAsync(id);
            TempData["Success"] = "Đã xóa đánh giá.";
            return RedirectToAction("Index", "Review");
        }
    }
}
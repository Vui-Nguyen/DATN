using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DATN.Services.Interfaces;
using DATN.Models.ViewModels;

namespace DATN.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

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

            return RedirectToAction("Details", "Product", new { id = model.ProductId });
        }

        // GET: /Review/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review == null) return NotFound();

            if (review.UserID != GetUserId())
                return Forbid();

            return View(review);
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
            return RedirectToAction("Details", "Product", new { id = review.ProductID });
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
            return RedirectToAction("Details", "Product", new { id = review.ProductID });
        }
    }
}
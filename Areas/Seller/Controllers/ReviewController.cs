using Microsoft.AspNetCore.Mvc;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DATN.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "Seller")]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }
        private int GetCurrentShopId()
        {
            var shopIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ShopId");
            if (shopIdClaim != null && int.TryParse(shopIdClaim.Value, out int shopId))
            {
                return shopId;
            }
            throw new Exception("Shop ID not found in user claims.");
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            int shopId = GetCurrentShopId();
            var result = await _reviewService.GetReviewsByShopIdAsync(shopId, page, pageSize);
            return View(result);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int reviewId, string replyContent)
        {
            int shopId = GetCurrentShopId(); 
            var result = await _reviewService.ReplyReviewAsync(reviewId, replyContent, shopId);

            if (!result.Success)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = result.Message;

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReviewBySeller(int reviewId)
        {
            int shopId = GetCurrentShopId();
            bool success = await _reviewService.DeleteBySellerAsync(reviewId, shopId);

            if (success)
                TempData["Success"] = "Đã xóa đánh giá của khách hàng.";
            else
                TempData["Error"] = "Không thể xóa đánh giá này.";

            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReply(int reviewId, string replyContent)
        {
            // Lấy ShopId từ Claims của Seller đã lưu lúc Login
            var shopIdClaim = User.FindFirst("ShopId")?.Value;
            if (string.IsNullOrEmpty(shopIdClaim) || !int.TryParse(shopIdClaim, out int shopId))
            {
                TempData["Error"] = "Không tìm thấy thông tin cửa hàng.";
                return RedirectToAction("Index");
            }

            var result = await _reviewService.UpdateReplyAsync(reviewId, shopId, replyContent);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Index");
        }
    }
}

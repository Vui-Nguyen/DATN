using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DATN.Services.Interfaces;
using DATN.Models.ViewModels;

namespace DATN.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "Seller")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: /Admin/Orders/Index
        // Danh sách đơn hàng (có thể lọc theo trạng thái, ngày, ...)
        public async Task<IActionResult> Index(string? status = null, int page = 1)
        {
            var orders = await _orderService.GetAllAsync(status, page);
            ViewBag.CurrentStatus = status;
            return View(orders);
        }

        // GET: /Admin/Orders/Details/5
        // Duyệt đơn - xem chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetAdminDetailAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST: /Admin/Orders/Approve/5
        // Duyệt đơn hàng (chuyển sang trạng thái Confirmed)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _orderService.UpdateStatusAsync(id, "Confirmed");
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Admin/Orders/UpdateStatus/5
        // Cập nhật trạng thái đơn hàng (Pending, Confirmed, Shipping, Delivered, Cancelled)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var result = await _orderService.UpdateStatusAsync(id, status);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
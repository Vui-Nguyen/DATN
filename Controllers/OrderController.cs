using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DATN.Services.Interfaces;
using DATN.Models.ViewModels;

namespace DATN.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: /Order/MyOrders
        public async Task<IActionResult> MyOrders(int page = 1)
        {
            var orders = await _orderService.GetByUserAsync(GetUserId(), page);
            return View(orders);
        }

        // GET: /Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetDetailAsync(id, GetUserId());
            if (order == null) return NotFound();

            return View(order);
        }

        // POST: /Order/CreateOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(CreateOrderViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Checkout", "Cart");

            var result = await _orderService.CreateAsync(GetUserId(), model);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Checkout", "Cart");
            }

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction(nameof(Details), new { id = result.OrderId });
        }

        // POST: /Order/CancelOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var result = await _orderService.CancelAsync(id, GetUserId());
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = "Đơn hàng đã được hủy.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
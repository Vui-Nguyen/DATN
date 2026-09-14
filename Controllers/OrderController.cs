using DATN.Models.ViewModels;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YourApp.Services.Implementations;

namespace DATN.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        public OrderController(IOrderService orderService, ICartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
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
        // GET: /Cart/Checkout
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreateOrder()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetCartAsync(userId);
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction(nameof(Index));
            }

            var model = await _orderService.BuildCheckoutModelAsync(userId);
            return View(model);
        }
        // POST: /Cart/Checkout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(CreateOrderViewModel model)
        {
            ModelState.Remove("CartItems");
            ModelState.Remove("UserAddresses");

            if (!ModelState.IsValid)
            {
                
                var displayModel = await _orderService.BuildCheckoutModelAsync(GetUserId());


                displayModel.AddressId = model.AddressId;
                displayModel.Note = model.Note;
                displayModel.PaymentMethod = model.PaymentMethod;

                return View(displayModel);
            }

            var result = await _orderService.CreateAsync(GetUserId(), model);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;


                var displayModel = await _orderService.BuildCheckoutModelAsync(GetUserId());
                displayModel.AddressId = model.AddressId;
                displayModel.Note = model.Note;
                displayModel.PaymentMethod = model.PaymentMethod;

                return View(displayModel);
            }

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction("Details", "Order", new { id = result.OrderId });
        }
    }
}
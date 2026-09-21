using DATN.Data;
using DATN.Models.ViewModels;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YourApp.Services.Implementations;

namespace DATN.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly AppDbContext  _context;

        public OrderController(IOrderService orderService, ICartService cartService, AppDbContext context)
        {
            _orderService = orderService;
            _cartService = cartService;
            _context = context;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: /Order/MyOrders
        public async Task<IActionResult> MyOrders(int page = 1)
        {

            var orders = await _orderService.GetByUserAsync(GetUserId(), page);
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetDetailAsync(id, GetUserId());

            if (order == null)
            {
                return NotFound();
            }
            var adr = await _context.Addresses.FirstOrDefaultAsync(a => a.AddressId == order.AddressID);
            ViewBag.Address = adr.AddressDetail;

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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreateOrder(List<int> selectedCartItemIds, int? voucherId, int? addressId)
        {
            // Không có sản phẩm được chọn -> quay về giỏ hàng, KHÔNG tự lấy cả giỏ
            if (selectedCartItemIds == null || !selectedCartItemIds.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            var model = await _orderService.BuildCheckoutModelAsync(GetUserId(), selectedCartItemIds, voucherId);

            if (model.CartItems == null || !model.CartItems.Any())
            {
                TempData["Error"] = "Không tìm thấy sản phẩm đã chọn trong giỏ hàng.";
                return RedirectToAction(nameof(Index));
            }

            // Giữ lại địa chỉ đang chọn sau khi đổi voucher 
            if (addressId.HasValue && model.UserAddresses.Any(a => a.AddressID == addressId.Value))
                model.AddressId = addressId.Value;

            return View(model);
        }

        // POST: /Cart/CreateOrder
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(CreateOrderViewModel model)
        {
            // Các danh sách chỉ dùng để hiển thị, không được gửi lên
            ModelState.Remove(nameof(model.CartItems));
            ModelState.Remove(nameof(model.UserAddresses));
            ModelState.Remove(nameof(model.AvailableVouchers));

            if (!ModelState.IsValid)
                return await RebuildCheckoutView(model);

            var result = await _orderService.CreateAsync(GetUserId(), model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Đặt hàng thất bại.");
                return await RebuildCheckoutView(model);
            }

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction("Details", "Order", new { id = result.OrderId });
        }

        // Dựng lại trang thanh toán khi POST lỗi (giữ nguyên sản phẩm, voucher, địa chỉ, ghi chú, phương thức)
        private async Task<IActionResult> RebuildCheckoutView(CreateOrderViewModel posted)
        {
            var display = await _orderService.BuildCheckoutModelAsync(
                GetUserId(), posted.SelectedCartItemIds, posted.SelectedVoucherId);

            display.AddressId = posted.AddressId;
            display.Note = posted.Note;
            display.PaymentMethod = posted.PaymentMethod;

            return View("CreateOrder", display);
        }
    }
}
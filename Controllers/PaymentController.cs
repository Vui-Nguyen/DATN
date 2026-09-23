using DATN.Data;
using DATN.Helpers;
using DATN.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public PaymentController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public IActionResult PaymentSuccess()
        {
            return View();
        }

        public IActionResult PaymentFail()
        {
            return View();
        }

        [HttpGet("Payment/PaymentCallback")]
        public async Task<IActionResult> PaymentCallback()
        {
            var vnPayData = Request.Query;
            var hashSecret = _configuration["VnPay:HashSecret"];

            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in vnPayData)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value!);
                }
            }

            long orderId = long.Parse(vnPayData["vnp_TxnRef"]!);
            long vnPayTranId = Convert.ToInt64(vnPayData["vnp_TransactionNo"]!);
            string vnPay_ResponseCode = vnPayData["vnp_ResponseCode"]!;
            string vnPay_SecureHash = vnPayData["vnp_SecureHash"]!;

            bool checkSignature = vnpay.ValidateSignature(vnPay_SecureHash, hashSecret);
            if (!checkSignature)
            {
                ViewBag.Message = "Có lỗi xảy ra trong quá trình xác thực chữ ký (Checksum failed).";
                return View("PaymentFail");
            }

            // Lấy đơn hàng kèm theo OrderItems và Payments để có thể hoàn kho khi thất bại
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            var payment = order?.Payments.FirstOrDefault();

            if (vnPay_ResponseCode == "00")
            {
                if (order != null) order.Status = "Pending";
                if (payment != null)
                {
                    payment.Status = "Success";
                    payment.TransactionId = vnPayTranId.ToString();
                    payment.GatewayResponse = "Thanh toán thành công qua VNPay";
                }
                await _context.SaveChangesAsync();

                ViewBag.Message = $"Thanh toán thành công đơn hàng #{orderId}!";
                return View("PaymentSuccess");
            }
            else // Thanh toán thất bại hoặc khách bấm hủy trên VNPAY
            {
                if (order != null)
                {
                    if (order.Status == "Pending")
                    {
                        order.Status = "CancelLed";

                        // HOÀN LẠI TỒN KHO CHO CÁC SẢN PHẨM TRONG ĐƠN HÀNG
                        foreach (var item in order.OrderItems)
                        {
                            var variant = await _context.ProductVariants.FindAsync(item.VariantId);
                            if (variant != null)
                            {
                                variant.Stock += item.Quantity; // Cộng lại số lượng vào kho
                            }
                        }

                        // Ghi lại lịch sử thay đổi trạng thái đơn hàng
                        _context.OrderStatusHistories.Add(new OrderStatusHistory
                        {
                            OrderId = order.OrderId,
                            Status = "Failed",
                            UpdatedAt = DateTime.Now,
                            Note = $"Thanh toán thất bại (Mã lỗi: {vnPay_ResponseCode})",
                            ChangedBy = null // Hệ thống tự động cập nhật từ VNPAY
                        });
                    }
                }

                if (payment != null)
                {
                    payment.Status = "Failed";
                    payment.GatewayResponse = $"Mã lỗi VNPay: {vnPay_ResponseCode}";
                }

                await _context.SaveChangesAsync();

                ViewBag.Message = $"Thanh toán thất bại (Mã lỗi: {vnPay_ResponseCode}).";
                return View("PaymentFail");
            }
        }
    }
}
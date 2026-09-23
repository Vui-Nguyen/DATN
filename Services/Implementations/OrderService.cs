using DATN.Areas.Seller.Models.DTOs;
using DATN.Data;
using DATN.Models;
using DATN.Models.DTOs;
using DATN.Models.Entities;
using DATN.Models.ViewModels;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using DATN.Helpers;
using System.Security.Claims;
using System.Threading.Tasks;
using static NuGet.Packaging.PackagingConstants;

namespace DATN.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public OrderService(AppDbContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<PagedResult<OrderDto>> GetByUserAsync(int userId, int page)
        {
            int pageSize = 5;
            var query = _context.Orders.Where(o => o.UserId == userId).OrderByDescending(o => o.OrderDate);
            int totalItems = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(o => new OrderDto
                {
                    OrderID = o.OrderId,
                    OrderDate = o.OrderDate ?? DateTime.Now,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                }).ToListAsync();

            return new PagedResult<OrderDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        public async Task<OrderDetailDto?> GetDetailAsync(int id, int userId)
        {
            var order = await _context.Orders
               .Include(o => o.OrderItems)
               .ThenInclude(i => i.Variant)
               .ThenInclude(v => v.Product)
               .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null) return null;

            return new OrderDetailDto
            {
                OrderID = order.OrderId,
                CustomerName = order.User?.FullName ?? "",
                OrderDate = order.OrderDate ?? DateTime.Now,
                TotalAmount = order.TotalAmount,
                ShippingFee = order.ShippingFee,
                DiscountAmount = order.DiscountAmount,
                Note = order.Note,
                Status = order.Status,
                AddressID = order.AddressId,
                OrderItems = order.OrderItems.Select(i => new OrderItemDto
                {
                    VariantID = i.VariantId,
                    ProductName = i.Variant?.Product?.ProductName + " (" + i.Variant?.VariantName + ")",
                    Quantity = i.Quantity,
                    Price = i.UnitPrice
                }).ToList()
            };
        }

        private static readonly string[] AllowedPaymentMethods = { "COD", "Banking" };
        private const decimal ShippingFeePerOrder = 30000m;

        private static decimal CalculateShippingFee(decimal itemsTotal)
            => itemsTotal > 0 ? ShippingFeePerOrder : 0m;

        private static decimal GetEligibleTotal(Voucher voucher, List<CartItem> items)
        {
            var eligible = voucher.ShopId == null
                ? items
                : items.Where(i => i.Variant?.Product?.ShopId == voucher.ShopId).ToList();

            return eligible.Sum(i => i.Quantity * (i.Variant?.Price ?? 0));
        }

        // Kiểm tra voucher + tính tiền giảm (tính ở server, không tin client)
        private async Task<(Voucher? Voucher, decimal Discount, string? Error)> ApplyVoucherAsync(
            int? voucherId, List<CartItem> items)
        {
            if (voucherId is null or <= 0)
                return (null, 0m, null);

            var now = DateTime.Now;
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v =>
                v.VoucherId == voucherId.Value
                && v.Quantity > 0
                && (v.StartDate == null || v.StartDate <= now)
                && (v.EndDate == null || v.EndDate >= now));

            if (voucher == null)
                return (null, 0m, "Voucher không hợp lệ, đã hết hạn hoặc hết lượt sử dụng.");

            var eligibleTotal = GetEligibleTotal(voucher, items);
            if (eligibleTotal <= 0)
                return (null, 0m, "Voucher không áp dụng cho các sản phẩm đã chọn.");

            var discount = Math.Round((eligibleTotal * (voucher.DiscountPercent ?? 0m)) / 100m, 0);
            return (voucher, discount, null);
        }


        public async Task<CreateOrderViewModel> BuildCheckoutModelAsync(
            int userId, List<int> selectedCartItemIds, int? voucherId)
        {
            selectedCartItemIds ??= new List<int>();

            var model = new CreateOrderViewModel
            {
                SelectedCartItemIds = selectedCartItemIds
            };

            // 1. Sản phẩm được chọn (phải thuộc giỏ hàng của user)
            var items = await _context.CartItems
                .Include(i => i.Variant).ThenInclude(v => v.Product)
                .Where(i => i.Cart.UserId == userId && selectedCartItemIds.Contains(i.CartItemId))
                .ToListAsync();

            model.CartItems = items.Select(i => new CartItemDto
            {
                ProductName = i.Variant?.Product?.ProductName ?? string.Empty,
                Price = i.Variant?.Price ?? 0,
                Quantity = i.Quantity
            }).ToList();

            model.ItemsTotal = items.Sum(i => i.Quantity * (i.Variant?.Price ?? 0));

            // 2. Sổ địa chỉ
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ToListAsync();

            model.UserAddresses = addresses.Select(a => new AddressDto
            {
                AddressID = a.AddressId,
                ReceiverName = a.ReceiverName,
                Phone = a.Phone,
                AddressDetail = a.AddressDetail,
                IsDefault = a.IsDefault
            }).ToList();

            var defaultAddr = addresses.FirstOrDefault(a => a.IsDefault == true) ?? addresses.FirstOrDefault();
            model.AddressId = defaultAddr?.AddressId ?? 0;

            // 3. Voucher: chỉ lấy voucher còn dùng được VÀ thuộc shop có sản phẩm trong đơn
            var shopIds = items
                .Select(i => (int?)i.Variant?.Product?.ShopId)
                .Distinct()
                .ToList();

            var now = DateTime.Now;
            var vouchers = await _context.Vouchers
                .Where(v => v.Quantity > 0
                         && (v.StartDate == null || v.StartDate <= now)
                         && (v.EndDate == null || v.EndDate >= now)
                         && (v.ShopId == null || shopIds.Contains(v.ShopId)))
                .ToListAsync();

            model.AvailableVouchers = vouchers.Select(v => new VoucherDto
            {
                VoucherID = v.VoucherId,
                VoucherCode = v.VoucherCode,
                DiscountPercent = v.DiscountPercent,
                EndDate = v.EndDate
            }).ToList();

            // 4. Áp voucher đang chọn (không hợp lệ thì bỏ qua khi chỉ hiển thị)
            var (voucher, discount, _) = await ApplyVoucherAsync(voucherId, items);
            model.SelectedVoucherId = voucher?.VoucherId;
            model.SelectedVoucherCode = voucher?.VoucherCode;
            model.DiscountAmount = discount;
            // 5. Tổng tiền
            model.ShippingFee = CalculateShippingFee(model.ItemsTotal);
            model.TotalAmount = Math.Max(0, model.ItemsTotal + model.ShippingFee - model.DiscountAmount);

            return model;
        }

        // Tạo đơn hàng
        public async Task<OrderResult> CreateAsync(int userId, CreateOrderViewModel model)
        {
            // 0. Kiểm tra đầu vào
            if (!AllowedPaymentMethods.Contains(model.PaymentMethod))
                return new OrderResult { Success = false, Message = "Phương thức thanh toán không hợp lệ." };

            if (model.SelectedCartItemIds == null || !model.SelectedCartItemIds.Any())
                return new OrderResult { Success = false, Message = "Không có sản phẩm nào được chọn để thanh toán." };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Địa chỉ
                Address? address;
                if (model.AddressId > 0)
                {
                    address = await _context.Addresses
                        .FirstOrDefaultAsync(a => a.AddressId == model.AddressId && a.UserId == userId);

                    if (address == null)
                        return new OrderResult { Success = false, Message = "Địa chỉ giao hàng không hợp lệ." };
                }
                else
                {
                    address = await _context.Addresses
                        .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault == true);

                    if (address == null)
                        return new OrderResult { Success = false, Message = "Vui lòng thiết lập hoặc chọn địa chỉ giao hàng." };
                }

                // 2. Các sản phẩm được chọn
                var selectedItems = await _context.CartItems
                    .Include(i => i.Variant).ThenInclude(v => v.Product)
                    .Where(i => i.Cart.UserId == userId && model.SelectedCartItemIds.Contains(i.CartItemId))
                    .ToListAsync();

                if (!selectedItems.Any())
                    return new OrderResult { Success = false, Message = "Không có sản phẩm nào được chọn để thanh toán." };

                // 3. Kiểm tra kho TRƯỚC khi tạo đơn
                var outOfStock = selectedItems.FirstOrDefault(i => i.Variant == null || i.Variant.Stock < i.Quantity);
                if (outOfStock != null)
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Sản phẩm {outOfStock.Variant?.Product?.ProductName} không đủ số lượng trong kho."
                    };

                // 4. Tính tiền hoàn toàn ở server
                decimal itemsTotal = selectedItems.Sum(i => i.Quantity * i.Variant!.Price);
                decimal shippingFee = CalculateShippingFee(itemsTotal);

                var (voucher, discount, voucherError) = await ApplyVoucherAsync(model.SelectedVoucherId, selectedItems);
                if (voucherError != null)
                    return new OrderResult { Success = false, Message = voucherError };

                decimal totalAmount = Math.Max(0, itemsTotal + shippingFee - discount);


                // 5. Tạo đơn hàng
                var order = new Order
                {
                    UserId = userId,
                    AddressId = address.AddressId,
                    OrderDate = DateTime.Now,
                    DiscountAmount = discount,
                    ShippingFee = shippingFee,
                    TotalAmount = totalAmount,
                    Note = model.Note,
                    Status = "Pending"
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethod = model.PaymentMethod,
                    Amount = totalAmount,
                    PaymentDate = DateTime.Now,
                    Status = model.PaymentMethod == "COD" ? "Pending" : "AwaitingPayment"
                };
                _context.Payments.Add(payment);

                var voucherUsage = new VoucherUsage
                {
                    UserId = userId,
                    VoucherId = voucher?.VoucherId ?? 0,
                    OrderId = order.OrderId,
                    UsedDate = DateTime.Now
                };
                if (voucher != null)
                {
                    _context.VoucherUsages.Add(voucherUsage);
                    voucher.Quantity -= 1;
                }
                await _context.SaveChangesAsync();
                // 6. Chi tiết đơn + trừ kho
                foreach (var item in selectedItems)
                {
                    item.Variant!.Stock -= item.Quantity;

                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Variant.Price
                    });
                }
                // 8. Xoá khỏi giỏ hàng các sản phẩm đã mua 
                _context.CartItems.RemoveRange(selectedItems);
                // 9. Sinh link thanh toán nếu chọn Banking
                string? paymentUrl = null;
                if (model.PaymentMethod == "Banking")
                {
                    var context = _httpContextAccessor.HttpContext;
                    if (context != null)
                    {
                        paymentUrl = CreateVnPayUrl(order.OrderId, totalAmount, context);
                    }
                }


                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OrderResult
                {
                    Success = true,
                    OrderId = order.OrderId,
                    PaymentUrl = paymentUrl
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new OrderResult
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình xử lý đơn hàng: " + ex.Message
                };
            }
        }
        public async Task<ServiceResult> CancelAsync(int id, int userId)
        {
            // 1. Tìm đơn hàng kèm theo OrderItems và Payments để hoàn kho và cập nhật thanh toán
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
                return new ServiceResult { Success = false, Message = "Đơn hàng không tồn tại." };

            // 2. Kiểm tra nếu trạng thái không phải Pending thì không cho hủy
            if (order.Status != "Pending")
                return new ServiceResult { Success = false, Message = "Đơn hàng không ở trạng thái chờ xử lý, không thể hủy." };

            // 3. Hoàn lại số lượng sản phẩm vào kho
            foreach (var item in order.OrderItems)
            {
                var variant = await _context.ProductVariants.FindAsync(item.VariantId);
                if (variant != null)
                {
                    variant.Stock += item.Quantity;
                }
            }

            // 4. Cập nhật trạng thái đơn hàng thành Cancelled 
            order.Status = "Cancelled";

            // 5. Cập nhật trạng thái thanh toán trong bảng Payments thành Failed/Hủy
            var payment = order.Payments.FirstOrDefault();
            if (payment != null)
            {
                payment.Status = "Failed";
                payment.GatewayResponse = "Khách hàng đã chủ động hủy đơn hàng";
            }

            // 6. Thêm lịch sử thay đổi trạng thái
            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = id,
                Status = "Cancelled",
                UpdatedAt = DateTime.Now,
                ChangedBy = userId
            });

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Hủy đơn hàng thành công." };
        }
        // Lấy danh sách đơn hàng cho Seller (có hỗ trợ lọc theo status nếu cần)
        public async Task<PagedResult<OrderSellerDto>> GetAllAsync(string? status, int page)
        {
            int pageSize = 10;
            int userId = 0;
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                int.TryParse(userIdClaim, out userId);
            }

            int shopID = await _context.Shops
                .Where(s => s.UserId == userId)
                .Select(s => s.ShopId)
                .FirstOrDefaultAsync();

            var query = _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Variant)
                            .ThenInclude(v => v.Product)
                    .Where(o => o.OrderItems.Any(oi => oi.Variant.Product.ShopId == shopID))
                    .AsQueryable();
            // Lọc theo trạng thái nếu có truyền vào
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            int totalItems = await query.CountAsync();
            var items = await query.OrderByDescending(o => o.OrderDate).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(o => new OrderSellerDto
                {
                    OrderID = o.OrderId,
                    CustomerName = o.User != null ? o.User.FullName : "Khách vãng lai",
                    OrderDate = o.OrderDate ?? DateTime.Now,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                }).ToListAsync();

            return new PagedResult<OrderSellerDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        public async Task<OrderDetailSellerDto?> GetSellerDetailAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Payments)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Variant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return null;

            return new OrderDetailSellerDto
            {
                OrderID = order.OrderId,
                CustomerName = order.User?.FullName ?? "",
                OrderDate = order.OrderDate ?? DateTime.Now,
                TotalAmount = order.TotalAmount,
                ShippingFee = order.ShippingFee,
                DiscountAmount = order.DiscountAmount,
                Note = order.Note,
                PaymentMethod = order.Payments.FirstOrDefault()?.PaymentMethod ?? "COD",
                Status = order.Status,
                OrderItems = order.OrderItems.Select(i => new OrderItemDto
                {
                    VariantID = i.VariantId,
                    ProductName = i.Variant?.Product?.ProductName + " (" + i.Variant?.VariantName + ")",
                    Quantity = i.Quantity,
                    Price = i.UnitPrice
                }).ToList()
            };
        }

        public async Task<ServiceResult> UpdateStatusAsync(int id, string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return new ServiceResult { Success = false, Message = "Không tìm thấy đơn hàng." };

            if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) && !order.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in order.OrderItems)
                {
                    var variant = await _context.ProductVariants.FindAsync(item.VariantId);
                    if (variant != null)
                    {
                        variant.Stock += item.Quantity;
                    }
                }
            }
            order.Status = status;

            var payment = order.Payments.FirstOrDefault();
            if (payment != null)
            {
                if (status.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
                {
                    payment.Status = "Success";
                    payment.GatewayResponse = "Thanh toán thành công (COD)";
                }
                else if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    payment.Status = "Failed";
                    payment.GatewayResponse = "Lỗi COD / Đơn hàng đã bị hủy";
                }
            }

            int? sellerId = null;
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId))
            {
                sellerId = parsedId;
            }

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = id,
                Status = status,
                UpdatedAt = DateTime.Now,
                ChangedBy = sellerId
            });

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Cập nhật trạng thái thành công." };
        }
        public string CreateVnPayUrl(int orderId, decimal amount, HttpContext context)
        {
            var timeNow = DateTime.Now;
            var payUrl = _configuration["VnPay:BaseUrl"];
            var tmnCode = _configuration["VnPay:TmnCode"];
            var hashSecret = _configuration["VnPay:HashSecret"];
            var returnUrl = _configuration["VnPay:ReturnUrl"];

            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", tmnCode);
            vnpay.AddRequestData("vnp_Amount", ((int)(amount * 100)).ToString()); // VNPay nhân 100 lần số tiền thực tế
            vnpay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang #{orderId}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderId.ToString()); // Dùng OrderId làm mã giao dịch để dễ đối soát

            return vnpay.CreateRequestUrl(payUrl, hashSecret);
        }
    }
}
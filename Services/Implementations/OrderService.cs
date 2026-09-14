using DATN.Areas.Seller.Models.DTOs;
using DATN.Data;
using DATN.Models;
using DATN.Models.DTOs;
using DATN.Models.Entities;
using DATN.Models.ViewModels;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using static NuGet.Packaging.PackagingConstants;

namespace DATN.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public OrderService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
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
                PaymentMethod = order.PaymentMethod,
                Note = order.Note,
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

        public async Task<CreateOrderViewModel> BuildCheckoutModelAsync(int userId)
        {
            // 1. Lấy danh sách địa chỉ (phần này của bạn đã rất chuẩn)
            var addresses = await _context.Addresses
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Select(a => new AddressDto
                {
                    AddressID = a.AddressId,
                    ReceiverName = a.ReceiverName,
                    Phone = a.Phone,
                    AddressDetail = a.AddressDetail,
                    IsDefault = a.IsDefault
                })
                .ToListAsync();

            var defaultAddress = addresses.FirstOrDefault(a => a.IsDefault == true) ?? addresses.FirstOrDefault();

            // 2. Lấy thêm thông tin giỏ hàng để hiển thị danh sách sản phẩm và tính tiền trên View
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Variant)
                .ThenInclude(v => v.Product) // Nếu cần lấy tên sản phẩm
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var cartItemsDto = cart?.CartItems?.Select(i => new CartItemDto
            {
                ProductName = i.Variant?.Product?.ProductName ?? "",
                VariantID = i.Variant?.VariantId ?? 0,
                Price = i.Variant?.Price ?? 0,
                Quantity = i.Quantity
            }).ToList() ?? new List<CartItemDto>();

            decimal itemsTotal = cartItemsDto.Sum(i => i.Price * i.Quantity);
            decimal shippingFee = 30000; // Phí ship mặc định
            decimal DiscountAmount = 0; // Hoan thien them

            // 3. Trả về ViewModel đầy đủ dữ liệu cho View
            return new CreateOrderViewModel
            {
                UserAddresses = addresses,
                AddressId = defaultAddress?.AddressID ?? 0,
                ReceiverName = defaultAddress?.ReceiverName ?? "",
                Phone = defaultAddress?.Phone ?? "",
                AddressDetail = defaultAddress?.AddressDetail ?? "",
                CartItems = cartItemsDto,
                ShippingFee = shippingFee,
                DiscountAmount = DiscountAmount,
                TotalAmount = itemsTotal + shippingFee - DiscountAmount,
            };
        }  
        public async Task<OrderResult> CreateAsync(int userId, CreateOrderViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                int addressIdToUse = model.AddressId;

                if (addressIdToUse <= 0)
                {
                    // Nếu user không chọn trên form, lấy địa chỉ mặc định
                    var defaultAddress = await _context.Addresses
                        .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault == true);

                    if (defaultAddress == null)
                        return new OrderResult { Success = false, Message = "Vui lòng thiết lập hoặc chọn địa chỉ giao hàng." };

                    addressIdToUse = defaultAddress.AddressId;
                }
                else
                {
                    var isValidAddress = await _context.Addresses
                        .AnyAsync(a => a.AddressId == addressIdToUse && a.UserId == userId);

                    if (!isValidAddress)
                        return new OrderResult { Success = false, Message = "Địa chỉ giao hàng không hợp lệ." };
                }


                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(i => i.Variant)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                    return new OrderResult { Success = false, Message = "Giỏ hàng của bạn đang trống." };

                // 2. Tính tiền chuẩn xác (bao gồm cả ship và giảm giá nếu có)
                decimal itemsTotal = cart.CartItems.Sum(i => i.Quantity * (i.Variant != null ? i.Variant.Price : 0));
                decimal totalAmount = itemsTotal + model.ShippingFee - model.DiscountAmount;

                // 1. Tạo bản ghi đơn hàng 
                var order = new Order
                {
                    UserId = userId,
                    AddressId = addressIdToUse, 
                    OrderDate = DateTime.Now,
                    DiscountAmount = model.DiscountAmount,
                    Note = model.Note, 
                    PaymentMethod = model.PaymentMethod,
                    ShippingFee = model.ShippingFee,
                    TotalAmount = totalAmount,
                    Status = "Pending"
                    
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 2. Chuyển đổi CartItems thành OrderItems 
                foreach (var item in cart.CartItems)
                {
                    if (item.Variant == null || item.Variant.Stock < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return new OrderResult { Success = false, Message = $"Sản phẩm không đủ số lượng trong kho." };
                    }

                    // Trừ kho
                    item.Variant.Stock -= item.Quantity;
                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Variant?.Price ?? 0
                    });
                }

                // 3. Xóa các mặt hàng trong giỏ sau khi đặt hàng
                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return new OrderResult { Success = true, OrderId = order.OrderId };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new OrderResult { Success = false, Message = "Đã xảy ra lỗi trong quá trình xử lý đơn hàng: " + ex.Message };
            }
        }

        public async Task<ServiceResult> CancelAsync(int id, int userId)
        {
            // 1. Tìm đơn hàng kèm theo các sản phẩm trong đơn (OrderItems) để hoàn kho
            var order = await _context.Orders
                .Include(o => o.OrderItems) 
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
                return new ServiceResult { Success = false, Message = "Đơn hàng không tồn tại." };

            // 2. Kiểm tra nếu trạng thái không phải Pending thì không cho hủy
            if (order.Status != "Pending")
                return new ServiceResult { Success = false, Message = "Đơn hàng không ở trạng thái chờ xử lý, không thể hủy." };

            // 3. Hoàn lại số lượng sản phẩm vào kho
            foreach (var item in order.OrderItems)
            {
                // Tìm biến thể dựa vào VariantId lưu trong OrderItem
                var variant = await _context.ProductVariants.FindAsync(item.VariantId);
                if (variant != null)
                {
                    variant.Stock += item.Quantity; // Cộng ngược số lượng tồn kho của biến thể
                }
            }

            // 4. Cập nhật trạng thái đơn hàng thành Cancelled (hoặc cập nhật trực tiếp trên bảng Order)
            order.Status = "Cancelled";

            // 5. Thêm lịch sử thay đổi trạng thái
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
        public async Task<PagedResult<OrderSelllerDto>> GetAllAsync(string? status, int page)
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
                .Select(o => new OrderSelllerDto
                {
                    OrderID = o.OrderId,
                    CustomerName = o.User != null ? o.User.FullName : "Khách vãng lai",
                    OrderDate = o.OrderDate ?? DateTime.Now,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                }).ToListAsync();

            return new PagedResult<OrderSelllerDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        // Đổi hàm Detail phía Admin sang Seller Detail
        public async Task<OrderDetailSellerDto?> GetSellerDetailAsync(int id)
        {
            var order = await _context.Orders
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
                PaymentMethod = order.PaymentMethod,
                Note = order.Note,
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

        // Cập nhật trạng thái đơn hàng do Seller thực hiện, có lưu vết sellerId
        public async Task<ServiceResult> UpdateStatusAsync(int id, string status)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return new ServiceResult { Success = false, Message = "Không tìm thấy đơn hàng." };

            // Cập nhật trạng thái mới trực tiếp vào bảng Order nếu có cột Status
            order.Status = status;

            int? sellerId = null;
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId))
            {
                sellerId = parsedId;
            }
            // Đồng thời ghi lịch sử vào OrderStatusHistory kèm ID của Seller thao tác[cite: 1]
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
    }
}
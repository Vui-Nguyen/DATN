using DATN.Models;
using DATN.Services;
using Microsoft.EntityFrameworkCore;
using DATN.Models.Entities;
using DATN.Models.DTOs;
using DATN.Services.Interfaces;
using DATN.Models.ViewModels;
using DATN.Data;

namespace YourApp.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CartDto> GetCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Variant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return new CartDto
            {
                CartID = cart.CartId,
                Items = cart.CartItems.Select(i => new CartItemDto
                {
                    CartItemID = i.CartItemId,
                    VariantID = i.VariantId,
                    ProductName = i.Variant?.Product?.ProductName + " - " + i.Variant?.VariantName,
                    Price = i.Variant != null ? i.Variant.Price : 0,
                    Quantity = i.Quantity
                }).ToList()
            };
        }

        public async Task AddToCartAsync(int userId, int variantId, int quantity)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var item = cart.CartItems.FirstOrDefault(i => i.VariantId == variantId);
            if (item == null)
            {
                _context.CartItems.Add(new CartItem { CartId = cart.CartId, VariantId = variantId, Quantity = quantity });
            }
            else
            {
                item.Quantity += quantity;
                _context.CartItems.Update(item);
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(int userId, int cartItemId, int quantity)
        {
            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.CartItemId == cartItemId && i.Cart.UserId == userId);

            if (item != null)
            {
                item.Quantity = quantity;
                _context.CartItems.Update(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveItemAsync(int userId, int cartItemId)
        {
            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.CartItemId == cartItemId && i.Cart.UserId == userId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CheckoutViewModel> BuildCheckoutModelAsync(int userId)
        {
            // Tìm địa chỉ mặc định của người dùng từ hệ thống bảng Addresses (nếu có trong sơ đồ DB)
            var defaultAddress = await _context.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault==true);

            return new CheckoutViewModel
            {
                ReceiverName = defaultAddress?.ReceiverName ?? "",
                Phone = defaultAddress?.Phone ?? "",
                AddressDetail = defaultAddress?.AddressDetail ?? ""
            };
        }

        public async Task<OrderResult> ProcessCheckoutAsync(int userId, CheckoutViewModel model)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Variant)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return new OrderResult { Success = false, Message = "Giỏ hàng của bạn đang trống." };

            decimal total = cart.CartItems.Sum(i => i.Quantity * (i.Variant != null ? i.Variant.Price : 0));

            // 1. Tạo bản ghi đơn hàng
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                // Giả định gán thông tin nhận hàng vào trường dữ liệu mở rộng
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 2. Chuyển đổi CartItems thành OrderItems
            foreach (var item in cart.CartItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity
                });
            }

            // 3. Xóa các mặt hàng trong giỏ sau khi đặt hàng
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return new OrderResult { Success = true, OrderId = order.OrderId };
        }
    }
}
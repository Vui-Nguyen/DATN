using DATN.Models;
using DATN.Services;
using Microsoft.EntityFrameworkCore;
using DATN.Models.Entities;
using DATN.Models.DTOs;
using DATN.Services.Interfaces;
using DATN.Models.ViewModels;
using DATN.Data;

namespace DATN.Services.Implementations
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
                .ThenInclude(p => p.Shop)
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
                    Quantity = i.Quantity,
                    ShopId = i.Variant?.Product?.ShopId ?? 0,
                    ShopName = i.Variant?.Product?.Shop?.ShopName ?? "Cửa hàng" 
                }).ToList()
            };
        }

        public async Task<ServiceResult> AddToCartAsync(int userId, int variantId, int quantity)
        {
            // 1. Lấy thông tin tồn kho của variant
            int stock = await _context.ProductVariants
                .Where(v => v.VariantId == variantId)
                .Select(v => v.Stock)
                .FirstOrDefaultAsync();

            // 2. Lấy giỏ hàng của user kèm theo các item bên trong
            var cart = await _context.Carts
                                 .Include(c => c.CartItems)
                                 .FirstOrDefaultAsync(c => c.UserId == userId);

            // 3. Kiểm tra xem sản phẩm này đã có trong giỏ hàng trước đó chưa
            var existingItem = cart?.CartItems.FirstOrDefault(i => i.VariantId == variantId);
            int currentQuantityInCart = existingItem?.Quantity ?? 0;

            // 4. Kiểm tra xem tổng số lượng (trong giỏ + số lượng thêm) có vượt quá tồn kho không
            if (currentQuantityInCart + quantity > stock)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = $"Số lượng vượt quá tồn kho cho phép. (Kho chỉ còn: {stock})"
                };
            }

            // 5. Nếu chưa có giỏ hàng thì tạo mới giỏ hàng
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync(); // Lưu để sinh ra CartId
            }

            // 6. Thêm mới hoặc cập nhật số lượng trong giỏ
            if (existingItem == null)
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.CartId,
                    VariantId = variantId,
                    Quantity = quantity
                });
            }
            else
            {
                existingItem.Quantity += quantity;
                _context.CartItems.Update(existingItem);
            }

            await _context.SaveChangesAsync();

            return new ServiceResult
            {
                Success = true,
                Message = "Thêm vào giỏ hàng thành công."
            };
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

       
    }
}
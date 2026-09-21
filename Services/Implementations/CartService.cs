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

       
    }
}
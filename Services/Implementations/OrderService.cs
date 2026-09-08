using DATN.Models;
using Microsoft.EntityFrameworkCore;
using System;
using DATN.Models.Entities;
using System.Threading.Tasks;
using DATN.Models.DTOs;
using DATN.Services.Interfaces;
using DATN.Models.ViewModels;
using DATN.Data;
using DATN.Areas.Admin.Models.DTOs;

namespace DATN.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
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
                    TotalAmount = o.TotalAmount
                }).ToListAsync();

            return new PagedResult<OrderDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        public async Task<OrderDetailDto?> GetDetailAsync(int id, int userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Variant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(o => o.UserId == id && o.UserId == userId);

            if (order == null) return null;

            return new OrderDetailDto
            {
                OrderID = order.OrderId,
                OrderDate =  order.OrderDate ?? DateTime.Now,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(i => new OrderItemDto
                {
                    VariantID = i.VariantId,
                    ProductName = i.Variant?.Product?.ProductName + " (" + i.Variant?.VariantName + ")",
                    Quantity = i.Quantity,
                    Price = i.Variant != null ? i.Variant.Price : 0
                }).ToList()
            };
        }

        public async Task<OrderResult> CreateAsync(int userId, CreateOrderViewModel model)
        {
            // Hàm xử lý tạo trực tiếp từ dữ liệu Model khẩn cấp
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = 0 // Tính dựa vào các mục được chọn chi tiết
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return new OrderResult { Success = true, OrderId = order.OrderId };
        }

        public async Task<ServiceResult> CancelAsync(int id, int userId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);
            if (order == null) return new ServiceResult { Success = false, Message = "Đơn hàng không tồn tại." };

            // Cập nhật trạng thái thông qua bảng phụ OrderStatusHistory trong Database Diagram của bạn
            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = id,
                Status = "Cancelled",
                UpdatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true };
        }

        public async Task<PagedResult<OrderAdminDto>> GetAllAsync(string? status, int page)
        {
            int pageSize = 10;
            var query = _context.Orders.Include(o => o.User).AsQueryable();

            int totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(o => new OrderAdminDto
                {
                    OrderID = o.OrderId,
                    CustomerName = o.User != null ? o.User.FullName : "Khách vãng lai",
                    OrderDate = o.OrderDate ?? DateTime.Now,
                    TotalAmount = o.TotalAmount
                }).ToListAsync();

            return new PagedResult<OrderAdminDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        public async Task<OrderDetailAdminDto?> GetAdminDetailAsync(int id)
        {
            var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return null;

            return new OrderDetailAdminDto
            {
                OrderID = order.OrderId,
                CustomerName = order.User?.FullName ?? "",
                OrderDate = order.OrderDate ?? DateTime.Now,
                TotalAmount = order.TotalAmount
            };
        }

        public async Task<ServiceResult> UpdateStatusAsync(int id, string status)
        {
            var orderExists = await _context.Orders.AnyAsync(o => o.OrderId == id);
            if (!orderExists) return new ServiceResult { Success = false, Message = "Không tìm thấy đơn hàng." };

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = id,
                Status = status,
                UpdatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Cập nhật trạng thái thành công." };
        }
    }
}
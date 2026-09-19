using DATN.Models;
using DATN.Services;
using Microsoft.EntityFrameworkCore;
using DATN.Models.Entities;
using DATN.Models.DTOs;
using DATN.Services.Interfaces;
using DATN.Data;
using DATN.Areas.Seller.Models.ViewModels;

namespace DATN.Services.Implementations
{
    public class VoucherService : IVoucherService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public VoucherService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<PagedResult<VoucherDto>> GetAllVouchersAsync(int pageNumber, int pageSize, bool includeInactive = true)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int parsedUserId = 0;
            if (userId != null)
            {
                int.TryParse(userId, out parsedUserId);
            }


            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.UserId == parsedUserId);

            int shopID = shop != null ? shop.ShopId : 0;

            var query = _context.Vouchers.Where(v => v.ShopId == shopID);

            if (!includeInactive)
            {
                query = query.Where(v => v.IsActive);
            }

            int totalItems = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VoucherDto
                {
                    VoucherID = v.VoucherId,
                    VoucherCode = v.VoucherCode,
                    DiscountPercent = v.DiscountPercent,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    Quantity = v.Quantity,
                    IsActive = v.IsActive,
                    ShopId = v.ShopId
                })
                .ToListAsync();

            return new PagedResult<VoucherDto>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalItems > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 0
            };
        }

        public async Task<VoucherDto> GetVoucherByIdAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return null;


            return new VoucherDto
            {
                VoucherID = voucher.VoucherId,
                VoucherCode = voucher.VoucherCode,
                DiscountPercent = voucher.DiscountPercent,
                StartDate = voucher.StartDate,
                EndDate = voucher.EndDate,
                ShopId = voucher.ShopId,
                Quantity = voucher.Quantity,
                IsActive = voucher.IsActive
            };
        }

        public async Task<bool> CreateVoucherAsync(VoucherDto voucherDto)
        {
            // Map DTO -> Entity
            var voucher = new Voucher
            {
                VoucherCode = voucherDto.VoucherCode,
                DiscountPercent = voucherDto.DiscountPercent,
                StartDate = voucherDto.StartDate,
                EndDate = voucherDto.EndDate,
                Quantity = voucherDto.Quantity,
                ShopId = voucherDto.ShopId,
                IsActive = true 
            };

            _context.Vouchers.Add(voucher);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateVoucherAsync(VoucherDto voucherDto)
        {
            var voucher = await _context.Vouchers.FindAsync(voucherDto.VoucherID);
            if (voucher == null) return false;

            // Map DTO -> Entity
            voucher.VoucherCode = voucherDto.VoucherCode;
            voucher.DiscountPercent = voucherDto.DiscountPercent;
            voucher.StartDate = voucherDto.StartDate;
            voucher.EndDate = voucherDto.EndDate;
            voucher.Quantity = voucherDto.Quantity;
            // Không cập nhật IsActive ở đây để tránh vô tình mở lại mã đã xóa

            _context.Vouchers.Update(voucher);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SoftDeleteVoucherAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return false;

            // Xóa mềm: Chỉ chuyển trạng thái
            voucher.IsActive = false;

            _context.Vouchers.Update(voucher);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> ActivateVoucherAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return false;

            // Kích hoạt lại voucher
            voucher.IsActive = true;

            _context.Vouchers.Update(voucher);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ApplyVoucherToOrderAsync(string voucherCode, int customerShopID, decimal orderTotalAmount)
        {
            // 1. Tìm voucher đang hoạt động và đúng là của shop bán sản phẩm đó phát hành
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.VoucherCode == voucherCode && v.ShopId == customerShopID && v.IsActive);

            if (voucher == null) return false; // Mã không tồn tại hoặc không thuộc shop này

            // 2. Kiểm tra thời hạn và số lượng
            if (DateTime.Now < voucher.StartDate || DateTime.Now > voucher.EndDate || voucher.Quantity <= 0)
            {
                return false; // Hết hạn hoặc hết lượt dùng
            }
            return true;
        }
    }
}
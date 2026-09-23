using DATN.Areas.Admin.Models.ViewModels;
using DATN.Data;
using DATN.Models.DTOs;
using DATN.Models.Entities;
using DATN.Models.Enums;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DATN.Services.Implementations
{
    public class BrandService : IBrandService
    {
        private readonly AppDbContext _context;

        public BrandService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<PagedResult<BrandDto>> GetAllPagedAsync(int pageIndex = 1, int pageSize = 10)
        {
            return await GetPagedBrandsInternalAsync(isApproved: true, pageIndex, pageSize);
        }

        public async Task<PagedResult<BrandDto>> GetPendingApprovalPagedAsync(int pageIndex = 1, int pageSize = 10)
        {
            return await GetPagedBrandsInternalAsync(isApproved: false, pageIndex, pageSize);
        }


        private async Task<PagedResult<BrandDto>> GetPagedBrandsInternalAsync(bool isApproved, int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Brands
                .AsNoTracking()
                .Where(b => b.IsApproved == isApproved);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = await query
                .OrderByDescending(b => b.BrandId)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BrandDto
                {
                    BrandID = b.BrandId,
                    BrandName = b.BrandName,
                    IsApproved = b.IsApproved,
                    CreatedByUserId = b.CreatedByUserId,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<BrandDto>
            {
                Items = items,
                CurrentPage = pageIndex,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }


        public async Task<IEnumerable<BrandDto>> GetAllAsync()
        {
            return await _context.Brands
                .AsNoTracking()
                .Where(b => b.IsApproved)
                .OrderBy(b => b.BrandName)
                .Select(b => new BrandDto
                {
                    BrandID = b.BrandId,
                    BrandName = b.BrandName
                })
                .ToListAsync();
        }


        public async Task<BrandDto?> GetByIdAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return null;

            return new BrandDto
            {
                BrandID = brand.BrandId,
                BrandName = brand.BrandName,
                IsApproved = brand.IsApproved,
                CreatedByUserId = brand.CreatedByUserId,
                CreatedAt = brand.CreatedAt
            };
        }


        public async Task<ServiceResult> CreateAsync(BrandViewModel model, string? createdByUserId = null, bool isApproved = true)
        {
            var trimmedName = model.BrandName?.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
                return new ServiceResult { Success = false, Message = "Tên thương hiệu không được để trống." };

            var exists = await _context.Brands.AnyAsync(b => b.BrandName.ToLower() == trimmedName.ToLower());
            if (exists)
                return new ServiceResult { Success = false, Message = "Thương hiệu này đã tồn tại trong hệ thống." };

            var brand = new Brand
            {
                BrandName = trimmedName,
                IsApproved = isApproved,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return new ServiceResult
            {
                Success = true,
                Message = isApproved ? "Thêm thương hiệu thành công." : "Đề xuất thương hiệu đã được gửi tới Admin duyệt."
            };
        }


        public async Task<ServiceResult> ApproveAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
                return new ServiceResult { Success = false, Message = "Thương hiệu không tồn tại." };

            if (brand.IsApproved)
                return new ServiceResult { Success = false, Message = "Thương hiệu này đã được phê duyệt trước đó." };

            brand.IsApproved = true;

            // Tìm các sản phẩm đang chờ duyệt (PendingApproval) gắn với thương hiệu này
            var pendingProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.BrandId == id && p.Status == ProductStatus.PendingApproval.ToString())
                .ToListAsync();

            foreach (var product in pendingProducts)
            {
                // Nếu danh mục của sản phẩm cũng đã được duyệt (hoặc null) thì kích hoạt sản phẩm
                if (product.Category == null || product.Category.IsApproved)
                {
                    product.Status = ProductStatus.Active.ToString();
                }
            }

            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Duyệt thương hiệu thành công." };
        }

        public async Task<ServiceResult> RejectAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
                return new ServiceResult { Success = false, Message = "Thương hiệu không tồn tại." };

            if (brand.IsApproved)
                return new ServiceResult { Success = false, Message = "Không thể từ chối thương hiệu đã được phê duyệt." };

            // Tìm các sản phẩm đang gắn với thương hiệu bị từ chối
            var relatedProducts = await _context.Products
                .Where(p => p.BrandId == id)
                .ToListAsync();

            foreach (var product in relatedProducts)
            {
                product.BrandId = null; 
                product.Status = ProductStatus.Rejected.ToString(); 
            }

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Đã từ chối đề xuất thương hiệu." };
        }

        public async Task<ServiceResult> UpdateAsync(int id, BrandViewModel model)
        {
            var trimmedName = model.BrandName?.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
                return new ServiceResult { Success = false, Message = "Tên thương hiệu không được để trống." };

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
                return new ServiceResult { Success = false, Message = "Thương hiệu không tồn tại." };

            var duplicate = await _context.Brands.AnyAsync(b => b.BrandId != id && b.BrandName.ToLower() == trimmedName.ToLower());
            if (duplicate)
                return new ServiceResult { Success = false, Message = "Tên thương hiệu đã trùng với một thương hiệu khác." };

            brand.BrandName = trimmedName;
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Cập nhật thương hiệu thành công." };
        }


        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var hasProducts = await _context.Products.AnyAsync(p => p.BrandId == id);
            if (hasProducts)
                return new ServiceResult { Success = false, Message = "Không thể xóa thương hiệu này vì đang có sản phẩm liên kết." };

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
                return new ServiceResult { Success = false, Message = "Thương hiệu không tồn tại." };

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Xóa thương hiệu thành công." };
        }
    }
}
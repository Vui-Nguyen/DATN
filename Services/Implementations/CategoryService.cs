using DATN.Areas.Admin.Models.ViewModels;
using DATN.Data;
using DATN.Models.DTOs;
using DATN.Models.Entities;
using DATN.Models.Enums;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DATN.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        // 1. Phân trang danh mục ĐÃ DUYỆT (Hiển thị trang quản lý danh mục chính)
        public async Task<PagedResult<CategoryDto>> GetAllPagedAsync(int pageIndex = 1, int pageSize = 10)
        {
            return await GetPagedCategoriesInternalAsync(isApproved: true, pageIndex, pageSize);
        }

        // 2. Phân trang danh mục CHỜ DUYỆT (Dành cho trang duyệt đề xuất của Admin)
        public async Task<PagedResult<CategoryDto>> GetPendingApprovalPagedAsync(int pageIndex = 1, int pageSize = 10)
        {
            return await GetPagedCategoriesInternalAsync(isApproved: false, pageIndex, pageSize);
        }

        // 3. Hàm phụ trợ dùng chung cho phân trang
        private async Task<PagedResult<CategoryDto>> GetPagedCategoriesInternalAsync(bool isApproved, int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Categories
                .AsNoTracking()
                .Where(c => c.IsApproved == isApproved);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = await query
                .OrderByDescending(c => c.CategoryId)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoryDto
                {
                    CategoryID = c.CategoryId,
                    CategoryName = c.CategoryName,
                    IsApproved = c.IsApproved,
                    CreatedByUserId = c.CreatedByUserId,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<CategoryDto>
            {
                Items = items,
                CurrentPage = pageIndex,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        // 4. Lấy tất cả danh mục đã duyệt (dùng đổ vào dropdown <select> form tạo sản phẩm)
        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsApproved)
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategoryDto
                {
                    CategoryID = c.CategoryId,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();
        }

        // 5. Lấy chi tiết danh mục theo Id
        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            return new CategoryDto
            {
                CategoryID = category.CategoryId,
                CategoryName = category.CategoryName,
                IsApproved = category.IsApproved,
                CreatedByUserId = category.CreatedByUserId,
                CreatedAt = category.CreatedAt
            };
        }

        // 6. Thêm mới danh mục (Admin tạo hoặc Seller đề xuất)
        public async Task<ServiceResult> CreateAsync(CategoryViewModel model, string? createdByUserId = null, bool isApproved = true)
        {
            var trimmedName = model.CategoryName?.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
                return new ServiceResult { Success = false, Message = "Tên danh mục không được để trống." };

            var exists = await _context.Categories.AnyAsync(c => c.CategoryName.ToLower() == trimmedName.ToLower());
            if (exists)
                return new ServiceResult { Success = false, Message = "Danh mục này đã tồn tại trong hệ thống." };

            var category = new Category
            {
                CategoryName = trimmedName,
                IsApproved = isApproved,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new ServiceResult
            {
                Success = true,
                Message = isApproved ? "Thêm danh mục thành công." : "Đề xuất danh mục đã được gửi tới Admin duyệt."
            };
        }

        // 7. Duyệt đề xuất danh mục
        public async Task<ServiceResult> ApproveAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return new ServiceResult { Success = false, Message = "Danh mục không tồn tại." };

            if (category.IsApproved)
                return new ServiceResult { Success = false, Message = "Danh mục này đã được phê duyệt trước đó." };

            category.IsApproved = true;

            // Tìm các sản phẩm đang chờ duyệt thuộc danh mục này
            var pendingProducts = await _context.Products
                .Include(p => p.Brand)
                .Where(p => p.CategoryId == id && p.Status == ProductStatus.PendingApproval.ToString())
                .ToListAsync();

            foreach (var product in pendingProducts)
            {
                // Nếu thương hiệu đi kèm cũng đã duyệt (hoặc sản phẩm không chọn thương hiệu) -> Kích hoạt bán
                if (product.Brand == null || product.Brand.IsApproved)
                {
                    product.Status = ProductStatus.Active.ToString();
                }
            }

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Duyệt danh mục thành công." };
        }

        // 8. Từ chối đề xuất danh mục
        public async Task<ServiceResult> RejectAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return new ServiceResult { Success = false, Message = "Danh mục không tồn tại." };

            if (category.IsApproved)
                return new ServiceResult { Success = false, Message = "Không thể từ chối danh mục đã được phê duyệt." };

            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == id)
                .ToListAsync();

            foreach (var product in relatedProducts)
            {
                product.Status = ProductStatus.Rejected.ToString();
            }
            if (!relatedProducts.Any())
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Đã từ chối đề xuất danh mục." };
        }
        public async Task<ServiceResult> UpdateAsync(int id, CategoryViewModel model)
        {
            var trimmedName = model.CategoryName?.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
                return new ServiceResult { Success = false, Message = "Tên danh mục không được để trống." };

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return new ServiceResult { Success = false, Message = "Danh mục không tồn tại." };

            var duplicate = await _context.Categories.AnyAsync(c => c.CategoryId != id && c.CategoryName.ToLower() == trimmedName.ToLower());
            if (duplicate)
                return new ServiceResult { Success = false, Message = "Tên danh mục đã trùng với một danh mục khác." };

            category.CategoryName = trimmedName;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Cập nhật danh mục thành công." };
        }

        // 10. Xóa danh mục
        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
                return new ServiceResult { Success = false, Message = "Không thể xóa danh mục này vì đang có sản phẩm liên kết." };

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return new ServiceResult { Success = false, Message = "Danh mục không tồn tại." };

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Xóa danh mục thành công." };
        }
    }
}
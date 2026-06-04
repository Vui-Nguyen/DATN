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
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDto { CategoryID = c.CategoryId, CategoryName = c.CategoryName })
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;
            return new CategoryDto { CategoryID = category.CategoryId, CategoryName = category.CategoryName };
        }

        public async Task<ServiceResult> CreateAsync(CategoryViewModel model)
        {
            var category = new Category { CategoryName = model.CategoryName };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true };
        }

        public async Task<ServiceResult> UpdateAsync(int id, CategoryViewModel model)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return new ServiceResult { Success = false, Message = "Danh mục không tồn tại." };

            category.CategoryName = model.CategoryName;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true };
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
                return new ServiceResult { Success = false, Message = "Không thể xóa danh mục này vì đã có sản phẩm thuộc danh mục." };

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return new ServiceResult { Success = false, Message = "Danh mục không tồn tại." };

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true };
        }
    }
}
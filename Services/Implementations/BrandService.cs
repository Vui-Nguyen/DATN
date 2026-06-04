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
    public class BrandService : IBrandService
    {
        private readonly AppDbContext _context;

        public BrandService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BrandDto>> GetAllAsync()
        {
            return await _context.Brands
                .Select(b => new BrandDto { BrandID = b.BrandId, BrandName = b.BrandName })
                .ToListAsync();
        }

        public async Task<BrandDto?> GetByIdAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return null;
            return new BrandDto { BrandID = brand.BrandId, BrandName = brand.BrandName };
        }

        public async Task<ServiceResult> CreateAsync(BrandViewModel model)
        {
            var brand = new Brand { BrandName = model.BrandName };
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true };
        }

        public async Task<ServiceResult> UpdateAsync(int id, BrandViewModel model)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return new ServiceResult { Success = false, Message = "Thương hiệu không tồn tại." };

            brand.BrandName = model.BrandName;
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true };
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var hasProducts = await _context.Products.AnyAsync(p => p.BrandId == id);
            if (hasProducts)
                return new ServiceResult { Success = false, Message = "Không thể xóa thương hiệu này vì đang liên kết với sản phẩm." };

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return new ServiceResult { Success = false, Message = "Thương hiệu không tồn tại." };

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true };
        }
    }
}
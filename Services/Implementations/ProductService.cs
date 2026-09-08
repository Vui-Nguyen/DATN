using DATN.Models;
using DATN.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using DATN.Models.Entities;
using DATN.Models.DTOs;
using DATN.Services.Interfaces;
using DATN.Data;
using DATN.Areas.Admin.Models.DTOs;
using DATN.Areas.Seller.Models.ViewModels;

namespace DATN.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ProductDto>> GetAllAsync(int page, int pageSize)
        {
            var query = _context.Products.Include(p => p.ProductImages).AsQueryable();
            int totalItems = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductDto
                {
                    ProductID = p.ProductId,
                    ProductName = p.ProductName,
                    Price = _context.ProductVariants.Where(v => v.ProductId == p.ProductId).Min(v => (decimal?)v.Price) ?? 0,
                    ImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().ImageUrl : "/images/default.jpg"
                }).ToListAsync();

            return new PagedResult<ProductDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        public async Task<ProductDetailDto?> GetDetailAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return null;

            var variants = await _context.ProductVariants.Where(v => v.ProductId == id).ToListAsync();

            return new ProductDetailDto
            {
                ProductID = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                CategoryName = product.Category?.CategoryName,
                BrandName = product.Brand?.BrandName,
                Images = product.ProductImages.Select(img => img.ImageUrl).ToList(),
                Variants = variants.Select(v => new VariantDto { VariantID = v.VariantId, VariantName = v.VariantName, Price = v.Price }).ToList()
            };
        }

        public async Task<PagedResult<ProductDto>> SearchAsync(string keyword, int page, int pageSize)
        {
            var query = _context.Products.Include(p => p.ProductImages)
                .Where(p => p.ProductName.Contains(keyword)).AsQueryable();

            int totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductDto
                {
                    ProductID = p.ProductId,
                    ProductName = p.ProductName,
                    Price = _context.ProductVariants.Where(v => v.ProductId == p.ProductId).Min(v => (decimal?)v.Price) ?? 0,
                    ImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().ImageUrl : "/images/default.jpg"
                }).ToListAsync();

            return new PagedResult<ProductDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        public async Task<PagedResult<ProductDto>> GetByCategoryAsync(int categoryId, int page, int pageSize)
        {
            var query = _context.Products.Include(p => p.ProductImages)
                .Where(p => p.CategoryId == categoryId).AsQueryable();

            int totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductDto
                {
                    ProductID = p.ProductId,
                    ProductName = p.ProductName,
                    Price = _context.ProductVariants.Where(v => v.ProductId == p.ProductId).Min(v => (decimal?)v.Price) ?? 0,
                    ImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().ImageUrl : "/images/default.jpg"
                }).ToListAsync();

            return new PagedResult<ProductDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        public async Task<PagedResult<ProductAdminDto>> GetAllAdminAsync(int page)
        {
            int pageSize = 10;
            var query = _context.Products.Include(p => p.Category).Include(p => p.Brand).AsQueryable();
            int totalItems = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductAdminDto
                {
                    ProductID = p.ProductId,
                    ProductName = p.ProductName,
                    CategoryName = p.Category != null ? p.Category.CategoryName : "",
                    BrandName = p.Brand != null ? p.Brand.BrandName : ""
                }).ToListAsync();

            return new PagedResult<ProductAdminDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        public async Task<ProductViewModel?> GetForEditAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            return new ProductViewModel
            {
                ProductName = product.ProductName,
                Description = product.Description,
                CategoryID = product.CategoryId,
                BrandID = (int) product.BrandId
            };
        }

        public async Task<ServiceResult> CreateAsync(ProductViewModel model, List<IFormFile>? images, string webRootPath)
        {
            var product = new Product
            {
                ProductName = model.ProductName,
                Description = model.Description,
                CategoryId = model.CategoryID,
                BrandId = model.BrandID
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            if (images != null && images.Count > 0)
            {
                await SaveProductImagesAsync(product.ProductId, images, webRootPath);
            }

            return new ServiceResult { Success = true };
        }

        public async Task<ServiceResult> UpdateAsync(int id, ProductViewModel model, List<IFormFile>? images, string webRootPath)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return new ServiceResult { Success = false, Message = "Sản phẩm không tồn tại." };

            product.ProductName = model.ProductName;
            product.Description = model.Description;
            product.CategoryId = model.CategoryID;
            product.BrandId = model.BrandID;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            if (images != null && images.Count > 0)
            {
                await SaveProductImagesAsync(product.ProductId, images, webRootPath);
            }

            return new ServiceResult { Success = true };
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return new ServiceResult { Success = false, Message = "Sản phẩm không tồn tại." };

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true };
        }

        private async Task SaveProductImagesAsync(int productId, List<IFormFile> images, string webRootPath)
        {
            var uploadDir = Path.Combine(webRootPath, "uploads", "products");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            foreach (var file in images)
            {
                if (file.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = productId,
                        ImageUrl = "/uploads/products/" + fileName
                    });
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
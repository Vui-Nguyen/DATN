using DATN.Areas.Seller.Models.DTOs;
using DATN.Areas.Seller.Models.ViewModels;
using DATN.Data;
using DATN.Models;
using DATN.Models.DTOs;
using DATN.Models.Entities;
using DATN.Models.Enums;
using DATN.Services;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Hosting; 
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DATN.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProductService(AppDbContext context, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> GetProductIdByVariantIdAsync(int variantId)
        {

            var variant = await _context.ProductVariants.FindAsync(variantId);
            return variant != null ? variant.ProductId : 0;
        }
        public async Task<PagedResult<ProductDto>> GetAllAsync(int page, int pageSize)
        {
            var query = _context.Products
                 .Where(p => !p.IsDeleted && p.Status == ProductStatus.Active.ToString())
                 .Include(p => p.ProductImages)
                 .Include(p => p.ProductVariants)
                 .AsQueryable();
            int totalItems = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductDto
                {
                    ProductID = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.ProductVariants.Min(v => (decimal?)v.Price) ?? 0,
                    ImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().ImageUrl : "/images/default.jpg",
                    Status = p.Status
                }).ToListAsync();

            return new PagedResult<ProductDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        public async Task<ProductDetailDto?> GetDetailAsync(int id)
        {
   
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Shop)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDeleted);

            if (product == null) return null;

            return new ProductDetailDto
            {
                ProductID = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                CategoryID = product.CategoryId, 
                BrandID = product.BrandId,       
                CategoryName = product.Category?.CategoryName ?? "Không có",
                BrandName = product.Brand?.BrandName ?? "Không có",
                ShopName = product.Shop?.ShopName ?? "Không xác định",
                ShopAvatar = product.Shop?.AvatarShop ?? "/images/default-shop.jpg",
                ShopId = product.ShopId,
                CreatedAt = product.CreatedAt,   
                Images = product.ProductImages.Select(img => img.ImageUrl).ToList(),
                Variants = product.ProductVariants.Select(v => new VariantDto
                {
                    VariantID = v.VariantId,
                    VariantName = v.VariantName,
                    Price = v.Price,
                    Stock = v.Stock            
                }).ToList()
            };
        }

        public async Task<PagedResult<ProductDto>> SearchAsync(string keyword, int page, int pageSize)
        {
            var query = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .Where(p => p.ProductName.Contains(keyword) && p.Status == ProductStatus.Active.ToString() && !p.IsDeleted)
                .AsQueryable();

            int totalItems = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductDto
                {
                    ProductID = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.ProductVariants.Min(v => (decimal?)v.Price) ?? 0,
                    ImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().ImageUrl : "/images/default.jpg",
                    Status = p.Status
                }).ToListAsync();

            return new PagedResult<ProductDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        public async Task<PagedResult<ProductDto>> GetByCategoryAsync(int categoryId, int page, int pageSize)
        {
            var query = _context.Products
                .Where(p => !p.IsDeleted && p.Status == ProductStatus.Active.ToString())
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .Where(p => p.CategoryId == categoryId)
                .AsQueryable();

            int totalItems = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductDto
                {
                    ProductID = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.ProductVariants.Min(v => (decimal?)v.Price) ?? 0,
                    ImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().ImageUrl : "/images/default.jpg",
                    Status = p.Status
                }).ToListAsync();

            return new PagedResult<ProductDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        public async Task<PagedResult<ProductDto>> GetAllSellerAsync(int page)
        {
            int shopId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirst("ShopId")?.Value);
            int pageSize = 10;

            var query = _context.Products
                .Where(p => !p.IsDeleted && p.ShopId == shopId)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Shop)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .AsQueryable();

            int totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ProductDto>
            {
                Items = items.Select(p => new ProductDto
                {
                    ProductID = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.ProductVariants.Min(v => (decimal?)v.Price) ?? 0,
                    ImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().ImageUrl : "/images/default.jpg",
                    Images = p.ProductImages.Select(img => img.ImageUrl).ToList(),
                    Status = p.Status
                }).ToList(),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };
        }
        public async Task<ProductViewModel?> GetForEditAsync(int id)
        {
            // Include thêm ProductVariants và ProductProductImages để lấy đầy đủ dữ liệu cũ lên form Edit
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return null;

            return new ProductViewModel
            {
                ProductID = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                CategoryID = product.CategoryId,
                BrandID = product.BrandId ?? 0,
                ShopID = product.ShopId,

                // Lấy danh sách đường dẫn ảnh cũ để hiển thị trực tiếp ra giao diện Edit
                Images = product.ProductImages.Select(img => img.ImageUrl).ToList(),

                // Ánh xạ danh sách biến thể cũ vào ViewModel để hiển thị ra các ô input khi sửa
                Variants = product.ProductVariants.Select(v => new ProductVariantViewModel
                {
                    VariantName = v.VariantName,
                    Price = v.Price,
                    Stock = v.Stock
                }).ToList()
            };
        }

        public async Task<ServiceResult> UpdateAsync(int id, ProductViewModel model, List<IFormFile>? newImages)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products
                    .Include(p => p.ProductVariants)
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    return new ServiceResult { Success = false, Message = "Sản phẩm không tồn tại." };
                }


                if (model.Variants == null || model.Variants.Count == 0)
                {
                    return new ServiceResult { Success = false, Message = "Vui lòng nhập ít nhất một phân loại sản phẩm." };
                }


                product.ProductName = model.ProductName;
                product.Description = model.Description;
                product.CategoryId = model.CategoryID;
                product.BrandId = model.BrandID == 0 ? null : model.BrandID;


                _context.ProductVariants.RemoveRange(product.ProductVariants);
                foreach (var v in model.Variants)
                {
                    _context.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = product.ProductId,
                        VariantName = string.IsNullOrWhiteSpace(v.VariantName) ? "Mặc định" : v.VariantName,
                        Price = v.Price,
                        Stock = v.Stock
                    });
                }


                var imagesToRemove = product.ProductImages
                    .Where(img => model.ExistingImages == null || !model.ExistingImages.Contains(img.ImageUrl))
                    .ToList();

                if (imagesToRemove.Any())
                {

                    foreach (var img in imagesToRemove)
                    {
                        var filePath = Path.Combine(_env.WebRootPath, img.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    _context.ProductImages.RemoveRange(imagesToRemove);
                }

                // Thêm các ảnh mới được chọn tải lên (nếu có)
                if (newImages != model.NewImages && newImages != null && newImages.Count > 0)
                {
                    await SaveProductImagesAsync(product.ProductId, newImages);
                }
                else if (model.NewImages != null && model.NewImages.Count > 0)
                {
                    await SaveProductImagesAsync(product.ProductId, model.NewImages);
                }

                // Lưu thay đổi và xác nhận Transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ServiceResult { Success = true, Message = "Cập nhật sản phẩm thành công." };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return new ServiceResult { Success = false, Message = "Lỗi cập nhật sản phẩm: " + errorMsg };
            }
        }


        public async Task<ServiceResult> CreateAsync(ProductViewModel model, List<IFormFile>? images)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return new ServiceResult { Success = false, Message = "Không tìm thấy thông tin người dùng." };
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var shop = await _context.Shops.FirstOrDefaultAsync(s => s.UserId == currentUserId);
                if (shop == null)
                {
                    return new ServiceResult { Success = false, Message = "Không tìm thấy cửa hàng của bạn." };
                }

                if (model.Variants == null || model.Variants.Count == 0)
                {
                    return new ServiceResult { Success = false, Message = "Vui lòng nhập ít nhất một phân loại sản phẩm." };
                }

                // Biến cờ kiểm tra xem người bán có đề xuất mới cần Admin phê duyệt hay không
                bool hasNewProposal = false;

                // 1. XỬ LÝ ĐỀ XUẤT DANH MỤC MỚI
                int finalCategoryId = model.CategoryID;
                if (model.CategoryID == 0)
                {
                    if (string.IsNullOrWhiteSpace(model.NewCategoryName))
                    {
                        return new ServiceResult { Success = false, Message = "Vui lòng nhập tên danh mục đề xuất." };
                    }

                    var categoryName = model.NewCategoryName.Trim();

                    var existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == categoryName.ToLower());

                    if (existingCategory != null)
                    {
                        finalCategoryId = existingCategory.CategoryId;
                        // Nếu danh mục có sẵn nhưng chưa được duyệt thì sản phẩm vẫn phải chờ duyệt
                        if (!existingCategory.IsApproved)
                        {
                            hasNewProposal = true;
                        }
                    }
                    else
                    {
                        var newCategory = new Category
                        {
                            CategoryName = categoryName,
                            IsApproved = false,
                            CreatedByUserId = currentUserId.ToString(),
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Categories.Add(newCategory);
                        await _context.SaveChangesAsync();

                        finalCategoryId = newCategory.CategoryId;
                        hasNewProposal = true; // Đánh dấu cần duyệt
                    }
                }

                // 2. XỬ LÝ ĐỀ XUẤT THƯƠNG HIỆU MỚI
                int? finalBrandId = model.BrandID;
                if (model.BrandID == 0)
                {
                    if (!string.IsNullOrWhiteSpace(model.NewBrandName))
                    {
                        var brandName = model.NewBrandName.Trim();

                        var existingBrand = await _context.Brands
                            .FirstOrDefaultAsync(b => b.BrandName.ToLower() == brandName.ToLower());

                        if (existingBrand != null)
                        {
                            finalBrandId = existingBrand.BrandId;
                            if (!existingBrand.IsApproved)
                            {
                                hasNewProposal = true;
                            }
                        }
                        else
                        {
                            var newBrand = new Brand
                            {
                                BrandName = brandName,
                                IsApproved = false,
                                CreatedByUserId = currentUserId.ToString(),
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.Brands.Add(newBrand);
                            await _context.SaveChangesAsync();

                            finalBrandId = newBrand.BrandId;
                            hasNewProposal = true; // Đánh dấu cần duyệt
                        }
                    }
                    else
                    {
                        finalBrandId = null;
                    }
                }

                // 3. TẠO SẢN PHẨM VỚI TRẠNG THÁI PHÙ HỢP
                var product = new Product
                {
                    ProductName = model.ProductName,
                    Description = model.Description,
                    CategoryId = finalCategoryId,
                    BrandId = finalBrandId,
                    ShopId = shop.ShopId,
                    CreatedAt = DateTime.Now,
                    Status = (hasNewProposal ? ProductStatus.PendingApproval : ProductStatus.Active).ToString()
                };

                _context.Products.Add(product);

                // 4. TẠO DANH SÁCH BIẾN THỂ
                foreach (var v in model.Variants)
                {
                    var variant = new ProductVariant
                    {
                        Product = product,
                        VariantName = string.IsNullOrWhiteSpace(v.VariantName) ? "Mặc định" : v.VariantName,
                        Price = v.Price,
                        Stock = v.Stock
                    };
                    _context.ProductVariants.Add(variant);
                }

                await _context.SaveChangesAsync();

                // 5. LƯU BỘ SƯU TẬP ẢNH
                if (images != null && images.Count > 0)
                {
                    await SaveProductImagesAsync(product.ProductId, images);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                string returnMessage = hasNewProposal
                    ? "Sản phẩm đã được tạo và đang chờ Admin duyệt danh mục/thương hiệu mới."
                    : "Tạo sản phẩm thành công.";

                return new ServiceResult { Success = true, Message = returnMessage };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return new ServiceResult { Success = false, Message = "Lỗi khi tạo sản phẩm: " + errorMsg };
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return new ServiceResult { Success = false, Message = "Không tìm thấy thông tin người dùng." };
            }

            try
            {
                // 1. Tìm shop của user hiện tại
                var shop = await _context.Shops.FirstOrDefaultAsync(s => s.UserId == currentUserId);
                if (shop == null)
                {
                    return new ServiceResult { Success = false, Message = "Không tìm thấy cửa hàng của bạn." };
                }

                // 2. Tìm sản phẩm theo ID và phải thuộc về Shop của user đó
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == id && p.ShopId == shop.ShopId);

                if (product == null)
                {
                    return new ServiceResult { Success = false, Message = "Không tìm thấy sản phẩm hoặc bạn không có quyền xóa sản phẩm này." };
                }

                // 3. Thực hiện Xóa mềm 
                product.IsDeleted = true;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                return new ServiceResult { Success = true, Message = "Xóa sản phẩm thành công." };
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return new ServiceResult { Success = false, Message = "Lỗi khi xóa: " + errorMsg };
            }
        }


        private async Task SaveProductImagesAsync(int productId, List<IFormFile> images)
        {
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            // Các định dạng ảnh được phép upload
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            foreach (var file in images)
            {
                if (file.Length > 0)
                {
                    var extension = Path.GetExtension(file.FileName).ToLower();

                    // Kiểm tra bảo mật
                    if (!allowedExtensions.Contains(extension)) continue;

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = productId,
                        // Lưu đường dẫn dạng tương đối vào DB
                        ImageUrl = "/uploads/products/" + fileName
                    });
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
using DATN.Areas.Admin.Models.DTOs;
using DATN.Areas.Seller.Models.ViewModels;
using DATN.Data;
using DATN.Models;
using DATN.Models.DTOs;
using DATN.Models.Entities;
using DATN.Services;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Hosting; // Thêm thư viện này cho IWebHostEnvironment
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

        public async Task<PagedResult<ProductDto>> GetAllAsync(int page, int pageSize)
        {
            var query = _context.Products
                .Where(p => p.IsDeleted == false)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants) 
                .AsQueryable();

            int totalItems = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductDto
                {
                    ProductID = p.ProductId,
                    ProductName = p.ProductName,
                    // Lấy giá Min từ Variants, dùng null-conditional ngắn gọn
                    Price = p.ProductVariants.Min(v => (decimal?)v.Price) ?? 0,
                    ImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().ImageUrl : "/images/default.jpg"
                }).ToListAsync();

            return new PagedResult<ProductDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }
        public async Task<ProductDetailDto?> GetDetailAsync(int id)
        {
            // Tối ưu: Include toàn bộ trong 1 query duy nhất
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Shop)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return null;

            return new ProductDetailDto
            {
                ProductID = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                CategoryID = product.CategoryId, // Thêm ID danh mục nếu cần dùng lại ở trang Chi tiết/Sửa
                BrandID = product.BrandId,       // Thêm ID thương hiệu
                CategoryName = product.Category?.CategoryName ?? "Không có",
                BrandName = product.Brand?.BrandName ?? "Không có",
                ShopName = product.Shop?.ShopName ?? "Không xác định",
                CreatedAt = product.CreatedAt,   // Thêm ngày đăng sản phẩm
                Images = product.ProductImages.Select(img => img.ImageUrl).ToList(),
                Variants = product.ProductVariants.Select(v => new VariantDto
                {
                    VariantID = v.VariantId,
                    VariantName = v.VariantName,
                    Price = v.Price,
                    Stock = v.Stock              // Thêm số lượng tồn kho của từng phân loại
                }).ToList()
            };
        }

        public async Task<PagedResult<ProductDto>> SearchAsync(string keyword, int page, int pageSize)
        {
            var query = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .Where(p => p.ProductName.Contains(keyword))
                .AsQueryable();

            int totalItems = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductDto
                {
                    ProductID = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.ProductVariants.Min(v => (decimal?)v.Price) ?? 0,
                    ImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().ImageUrl : "/images/default.jpg"
                }).ToListAsync();

            return new PagedResult<ProductDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        public async Task<PagedResult<ProductDto>> GetByCategoryAsync(int categoryId, int page, int pageSize)
        {
            var query = _context.Products
                .Where(p => p.IsDeleted == false)
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
                    ImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().ImageUrl : "/images/default.jpg"
                }).ToListAsync();

            return new PagedResult<ProductDto> { Items = items, CurrentPage = page, TotalPages = (int)Math.Ceiling((double)totalItems / pageSize) };
        }

        public async Task<PagedResult<ProductDto>> GetAllSellerAsync(int page)
        {
            int shopId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirst("ShopId")?.Value);
            int pageSize = 10;

            var query = _context.Products
                .Where(p => p.IsDeleted == false && p.ShopId == shopId)
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
                    Images = p.ProductImages.Select(img => img.ImageUrl).ToList()
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

                // Kiểm tra xem có nhập ít nhất 1 biến thể nào không
                if (model.Variants == null || model.Variants.Count == 0)
                {
                    return new ServiceResult { Success = false, Message = "Vui lòng nhập ít nhất một phân loại sản phẩm." };
                }

                // 1. Cập nhật thông tin cơ bản
                product.ProductName = model.ProductName;
                product.Description = model.Description;
                product.CategoryId = model.CategoryID;
                product.BrandId = model.BrandID == 0 ? null : model.BrandID;

                // 2. Cập nhật danh sách Variants (Xóa toàn bộ variants cũ, thêm mới các variants từ form gửi lên)
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

                // 3. Xử lý Hình ảnh (Giữ lại ảnh có trong ExistingImages, xóa ảnh bị gỡ khỏi giao diện và thêm ảnh mới upload)
                var imagesToRemove = product.ProductImages
                    .Where(img => model.ExistingImages == null || !model.ExistingImages.Contains(img.ImageUrl))
                    .ToList();

                if (imagesToRemove.Any())
                {
                    // Xóa file vật lý trên ổ cứng nếu muốn tối ưu dung lượng server
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

        // Kiểm tra xem người dùng có nhập ít nhất 1 biến thể nào không
        if (model.Variants == null || model.Variants.Count == 0)
        {
            return new ServiceResult { Success = false, Message = "Vui lòng nhập ít nhất một phân loại sản phẩm." };
        }

        // 1. Tạo thông tin cơ bản của sản phẩm
        var product = new Product
        {
            ProductName = model.ProductName,
            Description = model.Description,
            CategoryId = model.CategoryID,
            BrandId = model.BrandID == 0 ? null : model.BrandID,
            ShopId = shop.ShopId,
            CreatedAt = DateTime.Now
        };

        _context.Products.Add(product);

        // 2. Tạo danh sách các biến thể từ Model gửi lên
        foreach (var v in model.Variants)
        {
            var variant = new ProductVariant
            {
                Product = product, // Liên kết với sản phẩm vừa tạo
                VariantName = string.IsNullOrWhiteSpace(v.VariantName) ? "Mặc định" : v.VariantName,
                Price = v.Price,
                Stock = v.Stock
            };
            _context.ProductVariants.Add(variant);
        }

        // BẮT BUỘC: Lưu thay đổi lần 1 để database cấp phát ProductId thật
        await _context.SaveChangesAsync();

        // 3. Xử lý lưu ảnh nếu có
        if (images != null && images.Count > 0)
        {
            await SaveProductImagesAsync(product.ProductId, images);
            await _context.SaveChangesAsync();
        }

        // Xác nhận Transaction khi tất cả các bước đều thành công
        await transaction.CommitAsync();

        return new ServiceResult { Success = true };
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

                // 3. Thực hiện Xóa mềm (Đổi trạng thái IsDeleted thành true)
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

        // ĐÃ SỬA: Bảo mật đuôi file, dùng chung _env.WebRootPath
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

                    // Kiểm tra bảo mật: Bỏ qua nếu không phải file ảnh
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
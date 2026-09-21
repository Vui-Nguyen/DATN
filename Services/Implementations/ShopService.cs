using DATN.Areas.Seller.Models.ViewModels;
using DATN.Data;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ShopService : IShopService
{
    private readonly AppDbContext _context;

    public ShopService(AppDbContext context)
    {
        _context = context;
    }

    // --- XEM ---
    public async Task<ShopResponseDto> GetShopByIdAsync(int shopId)
    {
        return await _context.Shops
            .Where(s => s.ShopId == shopId)
            .Select(s => new ShopResponseDto
            {
                ShopID = s.ShopId,
                UserID = s.UserId,
                ShopName = s.ShopName,
                Description = s.Description,
                CreatedAt = s.CreatedAt,
                IsLocked = s.IsLocked,
                AvatarShop = s.AvatarShop
            })
            .FirstOrDefaultAsync();
    }

    // --- SỬA ---
    public async Task<bool> UpdateShopAsync(int shopId, ShopViewModel updateDto)
    {
        var shop = await _context.Shops.FindAsync(shopId);
        if (shop == null)
        {
            return false; // Không tìm thấy shop
        }

        // --- XỬ LÝ UPLOAD ẢNH MỚI VÀ XÓA ẢNH CŨ ---
        if (updateDto.AvatarShopFile != null && updateDto.AvatarShopFile.Length > 0)
        {
            // 1. Xóa file ảnh cũ nếu shop đã từng có ảnh và file đó thực sự tồn tại trong wwwroot
            if (!string.IsNullOrEmpty(shop.AvatarShop))
            {

                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", shop.AvatarShop.TrimStart('/'));

                if (File.Exists(oldFilePath))
                {
                    try
                    {
                        File.Delete(oldFilePath);   
                    }
                    catch (Exception ex)
                    {
                        
                        Console.WriteLine($"Không thể xóa file cũ: {ex.Message}");
                    }
                }
            }

    
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(updateDto.AvatarShopFile.FileName);
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "shops");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await updateDto.AvatarShopFile.CopyToAsync(stream);
            }

    
            shop.AvatarShop = "/images/shops/" + fileName;
        }

        // Cập nhật các thông tin khác
        shop.ShopName = updateDto.ShopName;
        shop.Description = updateDto.Description;

        await _context.SaveChangesAsync();

        return true;
    }

}
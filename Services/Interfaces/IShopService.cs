using System.Threading.Tasks;
using System.Collections.Generic;
using DATN.Models.DTOs;
using DATN.Areas.Admin.Models.ViewModels;
using DATN.Areas.Seller.Models.ViewModels;

namespace DATN.Services.Interfaces
{
    public interface IShopService
    {
        // Chức năng Xem
        Task<ShopResponseDto> GetShopByIdAsync(int shopId);

        // Chức năng Sửa
        Task<bool> UpdateShopAsync(int shopId, ShopViewModel model);
    }
}
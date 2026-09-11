using DATN.Models.DTOs;
using DATN.Models.Entities;
using DATN.Models.ViewModels;

namespace DATN.Services.Interfaces
{
    public interface ISellerService
    {
        Task<ServiceResult> ApproveSellerAsync(int id);
        Task<ServiceResult> RegisterSellerAsync(int userId, RegisterSellerViewModel model);
        Task<SellerProfile?> GetProfileByUserIdAsync(int userId);
        Task<bool> UpdateProfileAsync(SellerProfile model);
    }
}

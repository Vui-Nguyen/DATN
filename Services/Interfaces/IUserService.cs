using System.Threading.Tasks;
using System.Collections.Generic;
using DATN.Models.DTOs;
using DATN.Models.ViewModels;

namespace DATN.Services.Interfaces
{
    public interface IUserService
    {
        // Client-side Methods
        Task<ServiceResult> RegisterAsync(RegisterViewModel model);
        Task<UserDto> AuthenticateAsync(string email, string password);
        Task<UserDto> GetByIdAsync(int id);
        Task<ServiceResult> UpdateProfileAsync(int id, ProfileViewModel model);
        Task<ServiceResult> ChangePasswordAsync(int id, string currentPassword, string newPassword);

        // Admin-side Methods
        Task<PagedResult<UserDto>> GetAllAsync(int page, string? keyword);
        Task<ServiceResult> ToggleLockAsync(int id);
        Task<ServiceResult> ChangeRoleAsync(int id, int roleId);
    }
}
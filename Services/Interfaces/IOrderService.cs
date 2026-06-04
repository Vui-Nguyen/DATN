using System.Threading.Tasks;
using DATN.Models.DTOs;
using DATN.Models.ViewModels;

namespace DATN.Services.Interfaces
{
    public interface IOrderService
    {
        // Client-side Methods
        Task<PagedResult<OrderDto>> GetByUserAsync(int userId, int page);
        Task<OrderDetailDto> GetDetailAsync(int id, int userId);
        Task<OrderResult> CreateAsync(int userId, CreateOrderViewModel model);
        Task<ServiceResult> CancelAsync(int id, int userId);

        // Admin-side Methods
        Task<PagedResult<OrderAdminDto>> GetAllAsync(string? status, int page);
        Task<OrderDetailAdminDto> GetAdminDetailAsync(int id);
        Task<ServiceResult> UpdateStatusAsync(int id, string status);
    }
}
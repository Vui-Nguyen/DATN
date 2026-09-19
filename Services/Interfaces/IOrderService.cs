using System.Threading.Tasks;
using DATN.Areas.Seller.Models.DTOs;
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
        Task<CreateOrderViewModel> BuildCheckoutModelAsync(int userId, int? selectedVoucherId = null);

        // Seller-side Methods
        Task<OrderDetailSellerDto?> GetSellerDetailAsync(int id);
        Task<PagedResult<OrderSelllerDto>> GetAllAsync(string? status, int page);
        Task<ServiceResult> UpdateStatusAsync(int id, string status);
    }
}
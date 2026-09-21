using DATN.Areas.Seller.Models.DTOs;
using DATN.Models.DTOs;
using DATN.Models.ViewModels;
using System.Threading.Tasks;

namespace DATN.Services.Interfaces
{
    public interface IReviewService
    {
        //seller
        Task<PagedResult<ReviewSellerDto>> GetReviewsByShopIdAsync(int shopId, int page, int pageSize);
        Task<ServiceResult> ReplyReviewAsync(int reviewId, string replyContent, int shopId);
        Task<bool> DeleteBySellerAsync(int reviewId, int shopId);
        Task<ServiceResult> UpdateReplyAsync(int reviewId, int sellerShopId, string newReplyContent);

        //Customer
        Task<PagedResult<OrderItemDto>> GetDeliveredProductsByUserIdAsync(int userId, int page, int pageSize);
        Task<ReviewDto> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(CreateReviewViewModel model);
        Task<ServiceResult> UpdateAsync(int id, EditReviewViewModel model);
        Task<bool> DeleteAsync(int id);
    }
}
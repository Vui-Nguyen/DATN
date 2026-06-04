using System.Threading.Tasks;
using DATN.Models.DTOs;
using DATN.Models.ViewModels;

namespace DATN.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDto> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(CreateReviewViewModel model);
        Task<ServiceResult> UpdateAsync(int id, EditReviewViewModel model);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
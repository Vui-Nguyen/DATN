using System.Threading.Tasks;
using System.Collections.Generic;
using DATN.Models.DTOs;
using DATN.Areas.Admin.Models.ViewModels;

namespace DATN.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<PagedResult<CategoryDto>> GetAllPagedAsync(int pageIndex = 1, int pageSize = 10);
        Task<PagedResult<CategoryDto>> GetPendingApprovalPagedAsync(int pageIndex = 1, int pageSize = 10);
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(CategoryViewModel model, string? createdByUserId = null, bool isApproved = true);
        Task<ServiceResult> ApproveAsync(int id);
        Task<ServiceResult> RejectAsync(int id);
        Task<ServiceResult> UpdateAsync(int id, CategoryViewModel model);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
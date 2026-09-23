using System.Threading.Tasks;
using System.Collections.Generic;
using DATN.Models.DTOs;
using DATN.Areas.Admin.Models.ViewModels;

namespace DATN.Services.Interfaces
{
    public interface IBrandService
    {
        Task<PagedResult<BrandDto>> GetAllPagedAsync(int pageIndex = 1, int pageSize = 10);

        Task<PagedResult<BrandDto>> GetPendingApprovalPagedAsync(int pageIndex = 1, int pageSize = 10);

        Task<IEnumerable<BrandDto>> GetAllAsync();
        Task<ServiceResult> RejectAsync(int id);

        Task<BrandDto?> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(BrandViewModel model, string? createdByUserId = null, bool isApproved = true);
        Task<ServiceResult> ApproveAsync(int id);
        Task<ServiceResult> UpdateAsync(int id, BrandViewModel model);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
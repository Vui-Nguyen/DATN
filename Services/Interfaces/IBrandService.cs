using System.Threading.Tasks;
using System.Collections.Generic;
using DATN.Models.DTOs;
using DATN.Areas.Admin.Models.ViewModels;

namespace DATN.Services.Interfaces
{
    public interface IBrandService
    {
        Task<IEnumerable<BrandDto>> GetAllAsync();
        Task<BrandDto> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(BrandViewModel model);
        Task<ServiceResult> UpdateAsync(int id, BrandViewModel model);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
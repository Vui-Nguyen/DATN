using System.Threading.Tasks;
using System.Collections.Generic;
using DATN.Models.DTOs;
using DATN.Models.ViewModels;

namespace DATN.Services.Interfaces
{
    public interface ICategoryService
    {
        // Chung & Admin Methods
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(CategoryViewModel model);
        Task<ServiceResult> UpdateAsync(int id, CategoryViewModel model);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
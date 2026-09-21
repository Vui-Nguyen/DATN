using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using DATN.Models.DTOs;
using DATN.Areas.Seller.Models.ViewModels;

namespace DATN.Services.Interfaces
{
    public interface IProductService
    {

        Task<PagedResult<ProductDto>> GetAllAsync(int page, int pageSize);
        Task<ProductDetailDto> GetDetailAsync(int id);
        Task<PagedResult<ProductDto>> SearchAsync(string keyword, int page, int pageSize);
        Task<PagedResult<ProductDto>> GetByCategoryAsync(int categoryId, int page, int pageSize);

        Task<int> GetProductIdByVariantIdAsync(int variantId);
        Task<PagedResult<ProductDto>> GetAllSellerAsync(int page);
        Task<ProductViewModel> GetForEditAsync(int id);
        Task<ServiceResult> CreateAsync(ProductViewModel model, List<IFormFile>? images);
        Task<ServiceResult> UpdateAsync(int id, ProductViewModel model, List<IFormFile>? images);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
using System.Threading.Tasks;
using System.Collections.Generic;
using DATN.Models.DTOs;
using DATN.Areas.Seller.Models.ViewModels;

namespace DATN.Services.Interfaces
{
    public interface IVoucherService
    {
        Task<PagedResult<VoucherDto>> GetAllVouchersAsync(int pageNumber, int pageSize, bool includeInactive = true);
        Task<VoucherDto> GetVoucherByIdAsync(int id);
        Task<bool> CreateVoucherAsync(VoucherDto voucherDto);
        Task<bool> UpdateVoucherAsync(VoucherDto voucherDto);
        Task<bool> SoftDeleteVoucherAsync(int id);
        Task<bool> ActivateVoucherAsync(int id);

        Task<bool> ApplyVoucherToOrderAsync(string voucherCode, int customerShopID, decimal orderTotalAmount);
    }
}
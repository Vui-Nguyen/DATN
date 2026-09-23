using System.Threading.Tasks;
using DATN.Models.DTOs;
using DATN.Models.ViewModels;

namespace DATN.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(int userId);
        Task<ServiceResult> AddToCartAsync(int userId, int variantId, int quantity);
        Task UpdateQuantityAsync(int userId, int cartItemId, int quantity);
        Task RemoveItemAsync(int userId, int cartItemId);

    }
}
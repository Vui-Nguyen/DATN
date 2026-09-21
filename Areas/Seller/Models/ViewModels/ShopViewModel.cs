using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DATN.Areas.Seller.Models.ViewModels
{
    public class ShopViewModel
    {
        public string ShopName { get; set; }
        public string Description { get; set; }
        public IFormFile? AvatarShopFile { get; set; }

        public string? AvatarShop { get; set; }

    }
}
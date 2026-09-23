using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DATN.Areas.Seller.Models.ViewModels
{
    public class ProductViewModel
    {

        public int ProductID { get; set; }

        public int ShopID { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(250, ErrorMessage = "Tên sản phẩm không vượt quá 250 ký tự.")]
        public string ProductName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục cho sản phẩm.")]
        public int CategoryID { get; set; }
        public string? NewCategoryName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thương hiệu cho sản phẩm.")]
        public int BrandID { get; set; }

        public string? NewBrandName { get; set; }
        public List<string> Images { get; set; } = new List<string>();

        public List<string> ExistingImages { get; set; } = new List<string>();

        public List<IFormFile>? NewImages { get; set; } = new List<IFormFile>();

        public List<ProductVariantViewModel> Variants { get; set; } = new List<ProductVariantViewModel>();
    }

    public class ProductVariantViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên phân loại.")]
        public string VariantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá phân loại.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho.")]
        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho phải lớn hơn hoặc bằng 0.")]
        public int Stock { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace DATN.Areas.Seller.Models.ViewModels
{
    public class ProductViewModel
    {
        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(250, ErrorMessage = "Tên sản phẩm không vượt quá 250 ký tự.")]
        public string ProductName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục cho sản phẩm.")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thương hiệu cho sản phẩm.")]
        public int BrandID { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace DATN.Areas.Admin.Models.ViewModels
{
    public class BrandViewModel
    {
        [Required(ErrorMessage = "Tên thương hiệu không được phép bỏ trống.")]
        [StringLength(100, ErrorMessage = "Tên thương hiệu không được vượt quá 100 ký tự.")]
        public string BrandName { get; set; } = string.Empty;
    }
}
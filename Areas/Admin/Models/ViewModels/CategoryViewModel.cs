using System.ComponentModel.DataAnnotations;

namespace DATN.Areas.Admin.Models.ViewModels
{
    public class CategoryViewModel
    {
        [Required(ErrorMessage = "Tên danh mục không được phép bỏ trống.")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự.")]
        public string CategoryName { get; set; } = string.Empty;
    }
}
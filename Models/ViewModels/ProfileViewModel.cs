using System.ComponentModel.DataAnnotations;

namespace DATN.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-8]{8})\b$", ErrorMessage = "Số điện thoại không hợp lệ tại Việt Nam.")]
        public string? Phone { get; set; }
    }
}
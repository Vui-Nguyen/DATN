using System.ComponentModel.DataAnnotations;

namespace DATN.Models.ViewModels
{
    public class ProfileViewModel
    {
        public int UserID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? RoleName { get; set; }

        public bool IsLocked { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        public string FullName { get; set; } = string.Empty;

        [RegularExpression(@"^(03|05|07|08|09)[0-9]{8}$", ErrorMessage = "Số điện thoại không đúng định dạng tại Việt Nam.")]
        public string? Phone { get; set; }
    }
}
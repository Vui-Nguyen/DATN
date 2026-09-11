using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models.ViewModels
{
    public class RegisterSellerViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tài khoản")]
        public string BankAccountNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên ngân hàng")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số CCCD")]
        public string IdentityCardNumber { get; set; }

        // Dùng IFormFile để nhận file upload
        [Required(ErrorMessage = "Vui lòng tải lên ảnh chân dung")]
        public IFormFile PortraitImage { get; set; }

        [Required(ErrorMessage = "Vui lòng tải lên CCCD mặt trước")]
        public IFormFile FrontIdentityImage { get; set; }

        [Required(ErrorMessage = "Vui lòng tải lên CCCD mặt sau")]
        public IFormFile BackIdentityImage { get; set; }
    }
}
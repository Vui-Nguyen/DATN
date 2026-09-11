using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DATN.Areas.Seller.Models
{
    public class SellerProfileViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tài khoản")]
        [MaxLength(50)]
        public string BankAccountNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên ngân hàng")]
        [MaxLength(100)]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số CCCD")]
        [MaxLength(20)]
        public string IdentityCardNumber { get; set; }

        // Dùng để hiển thị ảnh cũ
        public string? PortraitImage { get; set; }
        public string? FrontIdentityImage { get; set; }
        public string? BackIdentityImage { get; set; }

        // Dùng để nhận file upload mới từ Form
        public IFormFile? PortraitImageFile { get; set; }
        public IFormFile? FrontIdentityImageFile { get; set; }
        public IFormFile? BackIdentityImageFile { get; set; }

        public int Status { get; set; }
    }
}
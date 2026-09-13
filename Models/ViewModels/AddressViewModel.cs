using System.ComponentModel.DataAnnotations;

namespace DATN.Models.ViewModels
{
    public class AddressViewModel
    {
        public int AddressID { get; set; }

        public int UserID { get; set; }

        [Required(ErrorMessage = "Tên người nhận không được để trống.")]
        [Display(Name = "Tên người nhận")]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Địa chỉ chi tiết không được để trống.")]
        [StringLength(255, ErrorMessage = "Địa chỉ chi tiết không được vượt quá 255 ký tự.")]
        [Display(Name = "Địa chỉ chi tiết")]
        public string AddressDetail { get; set; }

        [Display(Name = "Đặt làm địa chỉ mặc định")]
        public bool IsDefault { get; set; }

        [RegularExpression(@"^(03|05|07|08|09)[0-9]{8}$", ErrorMessage = "Số điện thoại không đúng định dạng tại Việt Nam.")]
        public string? Phone { get; set; }
    }
}
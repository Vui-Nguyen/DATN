using System.ComponentModel.DataAnnotations;

using DATN.Models.DTOs;

namespace DATN.Models.ViewModels

{
    public class CheckoutViewModel
    {
        public int SelectedAddressId { get; set; }

        // Danh sách địa chỉ để hiển thị ra dropdown hoặc radio button cho người dùng chọn
        public List<AddressDto> UserAddresses { get; set; } = new();
        [Display(Name = "Tên người nhận hàng")]
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận hàng.")]
        public string ReceiverName { get; set; } = string.Empty;
        [Display(Name = "Số điện thoại nhận hàng")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại nhận hàng.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string Phone { get; set; } = string.Empty;
        
        [Display(Name = "Chi tiết địa chỉ giao hàng")]
        [Required(ErrorMessage = "Vui lòng nhập chi tiết địa chỉ giao hàng.")]
        public string AddressDetail { get; set; } = string.Empty;

        public string? PaymentMethod { get; set; } = "COD"; // Mặc định là thanh toán khi nhận hàng
    }
}
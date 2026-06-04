using System.ComponentModel.DataAnnotations;

namespace DATN.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận hàng.")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại nhận hàng.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập chi tiết địa chỉ giao hàng.")]
        public string AddressDetail { get; set; } = string.Empty;

        public string? PaymentMethod { get; set; } = "COD"; // Mặc định là thanh toán khi nhận hàng
    }
}
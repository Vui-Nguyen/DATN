using System.ComponentModel.DataAnnotations;

using DATN.Models.DTOs;

namespace DATN.Models.ViewModels

{
    public class CreateOrderViewModel
    {
        public int AddressId { get; set; }

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
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? Note { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
        public string PaymentMethod { get; set; }
        public List<CartItemDto> CartItems { get; set; }
        public int? SelectedVoucherId { get; set; } 
        public string? SelectedVoucherCode { get; set; }
        public List<VoucherDto> AvailableVouchers { get; set; } = new List<VoucherDto>();
    }
}
using System.ComponentModel.DataAnnotations;
using DATN.Models.DTOs;

namespace DATN.Models.ViewModels
{
    public class CreateOrderViewModel
    {
        public int AddressId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public string PaymentMethod { get; set; } = "COD";

        public string? Note { get; set; }

        public int? SelectedVoucherId { get; set; }

        public List<int> SelectedCartItemIds { get; set; } = new();

        public List<AddressDto> UserAddresses { get; set; } = new();
        public List<CartItemDto> CartItems { get; set; } = new();
        public List<VoucherDto> AvailableVouchers { get; set; } = new();

        public string ReceiverName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressDetail { get; set; } = string.Empty;

        public string? SelectedVoucherCode { get; set; }
        public decimal ItemsTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
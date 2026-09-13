using System;
using System.Collections.Generic;

namespace DATN.Models.Entities;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public int AddressId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Status { get; set; }
    public string? Note { get; set; }             // Ghi chú của khách hàng

    public decimal ShippingFee { get; set; } = 0;   // Phí vận chuyển

    public decimal DiscountAmount { get; set; } = 0;// Số tiền giảm giá từ voucher

    public string? PaymentMethod { get; set; }      // Phương thức thanh toán (COD, BankTransfer...)

    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
}

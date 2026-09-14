using System;
using System.Collections.Generic;
using DATN.Models.DTOs;

namespace DATN.Areas.Seller.Models.DTOs
{
    public class OrderSelllerDto
    {
        public int OrderID { get; set; }

        public string Status { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }


    public class OrderDetailSellerDto
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public decimal ShippingFee { get; set; }
        public string PaymentMethod { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Note { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }
}
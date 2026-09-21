using System;
using System.Collections.Generic;

namespace DATN.Models.DTOs
{

    public class OrderDetailDto
    {
        public int AddressID { get; set; } = 0;
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

    public class OrderItemDto
    {
        public int VariantID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }


        public bool HasReviewed { get; set; }
        public int? ReviewId { get; set; }
    }

  

}
using System.Collections.Generic;

namespace DATN.Models.DTOs
{
    public class CartDto
    {
        public int CartID { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }

    public class CartItemDto
    {
        public int CartItemID { get; set; }
        public int VariantID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }
}
using Microsoft.AspNetCore.Mvc;

namespace DATN.Models.DTOs
{
    public class VoucherDto
    {
        public int VoucherID { get; set; }
        public string VoucherCode { get; set; }
        public int? DiscountPercent { get; set; }
        public DateTime? StartDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Quantity { get; set; }

        public int ShopId { get; set; }
    }
}

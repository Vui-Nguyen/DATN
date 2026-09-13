namespace DATN.Areas.Seller.Models.DTOs
{
    public class SellerDashboardDto
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
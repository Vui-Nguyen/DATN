namespace DATN.Areas.Seller.Models.DTOs
{
    public class ReviewSellerDto
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string? Reply { get; set; }
        public DateTime? RepliedAt { get; set; }
    }
}
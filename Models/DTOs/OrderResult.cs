namespace DATN.Models.DTOs
{
    public class OrderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int OrderId { get; set; } // Dùng để chuyển hướng sang trang chi tiết đơn hàng vừa tạo [cite: 270, 316]
    }
}
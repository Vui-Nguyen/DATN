namespace DATN.Models.DTOs
{
    public class OrderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string? PaymentUrl { get; set; }
    }
}
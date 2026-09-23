namespace DATN.Models.DTOs
{

    public class BrandDto
    {
        public int BrandID { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
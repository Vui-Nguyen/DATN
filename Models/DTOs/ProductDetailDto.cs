using System.Collections.Generic;

namespace DATN.Models.DTOs
{
    public class ProductDetailDto
    {
        public string ShopName { get; set; } = string.Empty;
        public int? ProductID { get; set; }
        public int? CategoryID { get; set; }
        public int? BrandID { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public List<VariantDto> Variants { get; set; } = new List<VariantDto>();
    }

    public class VariantDto
    {
        public int VariantID { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public int? Stock { get; set; }
    }
}
namespace DATN.Models.DTOs
{
    public class ProductDto
    {
        public List<string> Images { get; set; } = new List<string>();
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}
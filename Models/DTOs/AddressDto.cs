using Microsoft.AspNetCore.Mvc;

namespace DATN.Models.DTOs
{
    public class AddressDto 
    {
        public int AddressID { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressDetail { get; set; } = string.Empty;
        public bool? IsDefault { get; set; }
    }
}

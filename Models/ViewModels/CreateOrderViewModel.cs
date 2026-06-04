using System.ComponentModel.DataAnnotations;

namespace DATN.Models.ViewModels
{
    public class CreateOrderViewModel
    {
        [Required]
        public int AddressID { get; set; }

        public string? Note { get; set; }
    }
}
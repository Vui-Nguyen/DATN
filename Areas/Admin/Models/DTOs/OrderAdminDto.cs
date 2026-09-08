using System;
using System.Collections.Generic;

namespace DATN.Areas.Admin.Models.DTOs
{
    public class OrderAdminDto
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }


    public class OrderDetailAdminDto
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
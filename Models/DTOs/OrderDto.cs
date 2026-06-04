using System;
using System.Collections.Generic;

namespace DATN.Models.DTOs
{
    public class OrderDto
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

}
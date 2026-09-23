using System;
using System.Collections.Generic;

namespace DATN.Models.Entities;

public partial class Brand
{
    public int BrandId { get; set; }

    public string BrandName { get; set; } = null!;

    public bool IsApproved { get; set; } = true;
    public string? CreatedByUserId { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

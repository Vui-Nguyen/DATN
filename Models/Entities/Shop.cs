using System;
using System.Collections.Generic;

namespace DATN.Models.Entities;

public partial class Shop
{
    public int ShopId { get; set; }

    public int UserId { get; set; }

    public string ShopName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }
    public bool IsLocked { get; set; } = false;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual User User { get; set; } = null!;
}

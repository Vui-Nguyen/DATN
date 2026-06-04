using System;
using System.Collections.Generic;

namespace DATN.Models.Entities;

public partial class Product
{
    public int ProductId { get; set; }

    public int ShopId { get; set; }

    public int CategoryId { get; set; }

    public int? BrandId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Shop Shop { get; set; } = null!;
}

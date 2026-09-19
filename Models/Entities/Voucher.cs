using System;
using System.Collections.Generic;

namespace DATN.Models.Entities;

public partial class Voucher
{
    public int VoucherId { get; set; }

    public string VoucherCode { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    public int? DiscountPercent { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? Quantity { get; set; }

    public int ShopId { get; set; }
    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
}

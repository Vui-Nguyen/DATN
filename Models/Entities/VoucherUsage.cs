using System;
using System.Collections.Generic;

namespace DATN.Models.Entities;

public partial class VoucherUsage
{
    public int UsageId { get; set; }

    public int VoucherId { get; set; }

    public int UserId { get; set; }

    public int OrderId { get; set; }

    public DateTime? UsedDate { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual Voucher Voucher { get; set; } = null!;
}

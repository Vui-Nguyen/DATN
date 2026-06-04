using System;
using System.Collections.Generic;

namespace DATN.Models.Entities;

public partial class OrderStatusHistory
{
    public int HistoryId { get; set; }

    public int OrderId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}

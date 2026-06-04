using System;
using System.Collections.Generic;

namespace DATN.Models.Entities;

public partial class Address
{
    public int AddressId { get; set; }

    public int UserId { get; set; }

    public string? ReceiverName { get; set; }

    public string? Phone { get; set; }

    public string? AddressDetail { get; set; }

    public bool? IsDefault { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; } = null!;
}

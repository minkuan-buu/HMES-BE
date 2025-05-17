using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class Order
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? UserAddressId { get; set; }

    public decimal TotalPrice { get; set; }

    public decimal? ShippingFee { get; set; }

    public string? ShippingOrderCode { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DeviceItem> DeviceItems { get; set; } = new List<DeviceItem>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;

    public virtual UserAddress? UserAddress { get; set; }
}

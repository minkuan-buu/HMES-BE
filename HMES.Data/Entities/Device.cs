using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class Device
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Attachment { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<DeviceItem> DeviceItems { get; set; } = new List<DeviceItem>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

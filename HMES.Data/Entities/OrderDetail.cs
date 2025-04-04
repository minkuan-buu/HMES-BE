﻿using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class OrderDetail
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid? ProductId { get; set; }

    public Guid? DeviceId { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Device? Device { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product? Product { get; set; }
}

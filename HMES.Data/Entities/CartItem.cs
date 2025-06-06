﻿using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class CartItem
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid CartId { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}

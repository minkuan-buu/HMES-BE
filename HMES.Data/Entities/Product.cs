using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? MainImage { get; set; }

    public Guid CategoryId { get; set; }

    public int Amount { get; set; }

    public decimal Price { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductAttachment> ProductAttachments { get; set; } = new List<ProductAttachment>();
}

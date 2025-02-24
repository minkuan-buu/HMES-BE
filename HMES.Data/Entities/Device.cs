using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class Device
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Attachment { get; set; }

    public string Status { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsOnline { get; set; }

    public string Serial { get; set; } = null!;

    public decimal Price { get; set; }

    public DateTime? WarrantyExpiryDate { get; set; }

    public virtual ICollection<NutritionReport> NutritionReports { get; set; } = new List<NutritionReport>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual User User { get; set; } = null!;
}

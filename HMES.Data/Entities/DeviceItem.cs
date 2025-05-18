using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class DeviceItem
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }

    public Guid? UserId { get; set; }

    public Guid? PlantId { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsOnline { get; set; }

    public string Serial { get; set; } = null!;

    public DateTime? WarrantyExpiryDate { get; set; }

    public DateTime? LastSeen { get; set; }

    public int RefreshCycleHours { get; set; }

    public Guid OrderId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? PhaseId { get; set; }

    public virtual Device Device { get; set; } = null!;

    public virtual ICollection<NutritionReport> NutritionReports { get; set; } = new List<NutritionReport>();

    public virtual Order Order { get; set; } = null!;

    public virtual GrowthPhase? Phase { get; set; }

    public virtual Plant? Plant { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual User? User { get; set; }
}

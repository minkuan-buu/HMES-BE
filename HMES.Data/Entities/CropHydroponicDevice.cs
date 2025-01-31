using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class CropHydroponicDevice
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Attachment { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<NutritionReport> NutritionReports { get; set; } = new List<NutritionReport>();

    public virtual User User { get; set; } = null!;
}

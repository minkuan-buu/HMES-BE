using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class NutritionReport
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Device Device { get; set; } = null!;

    public virtual ICollection<NutritionReportDetail> NutritionReportDetails { get; set; } = new List<NutritionReportDetail>();
}

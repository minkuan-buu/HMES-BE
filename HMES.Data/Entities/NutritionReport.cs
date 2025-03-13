using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class NutritionReport
{
    public Guid Id { get; set; }

    public Guid DeviceItemId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual DeviceItem DeviceItem { get; set; } = null!;

    public virtual ICollection<NutritionReportDetail> NutritionReportDetails { get; set; } = new List<NutritionReportDetail>();
}

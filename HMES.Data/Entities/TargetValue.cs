using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class TargetValue
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public decimal MinValue { get; set; }

    public decimal MaxValue { get; set; }

    public virtual ICollection<NutritionReportDetail> NutritionReportDetails { get; set; } = new List<NutritionReportDetail>();

    public virtual ICollection<TargetOfPlant> TargetOfPlants { get; set; } = new List<TargetOfPlant>();
}

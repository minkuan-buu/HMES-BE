using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class NutritionReportDetail
{
    public Guid Id { get; set; }

    public Guid NutritionId { get; set; }

    public string Type { get; set; } = null!;

    public decimal Record { get; set; }

    public string Status { get; set; } = null!;

    public virtual NutritionReport Nutrition { get; set; } = null!;
}

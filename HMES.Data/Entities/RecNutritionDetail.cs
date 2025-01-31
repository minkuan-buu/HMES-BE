using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class RecNutritionDetail
{
    public Guid Id { get; set; }

    public Guid RecNutritionId { get; set; }

    public string Type { get; set; } = null!;

    public decimal Record { get; set; }

    public string Status { get; set; } = null!;

    public virtual RecNutrition RecNutrition { get; set; } = null!;
}

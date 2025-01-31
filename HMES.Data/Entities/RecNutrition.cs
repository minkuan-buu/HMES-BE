using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class RecNutrition
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<RecNutritionDetail> RecNutritionDetails { get; set; } = new List<RecNutritionDetail>();
}

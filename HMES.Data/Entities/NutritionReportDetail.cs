﻿using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class NutritionReportDetail
{
    public Guid Id { get; set; }

    public Guid TargetValueId { get; set; }

    public Guid NutritionId { get; set; }

    public decimal RecordValue { get; set; }

    public virtual NutritionReport Nutrition { get; set; } = null!;

    public virtual TargetValue TargetValue { get; set; } = null!;
}

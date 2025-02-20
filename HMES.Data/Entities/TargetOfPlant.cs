using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class TargetOfPlant
{
    public Guid Id { get; set; }

    public Guid TargetValueId { get; set; }

    public Guid PlantId { get; set; }

    public virtual Plant Plant { get; set; } = null!;

    public virtual TargetValue TargetValue { get; set; } = null!;
}

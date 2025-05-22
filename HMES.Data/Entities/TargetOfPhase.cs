using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class TargetOfPhase
{
    public Guid Id { get; set; }

    public Guid TargetValueId { get; set; }

    public Guid PlantOfPhaseId { get; set; }

    public virtual PlantOfPhase PlantOfPhase { get; set; } = null!;

    public virtual TargetValue TargetValue { get; set; } = null!;
}

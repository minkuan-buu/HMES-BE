using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class PlantOfPhase
{
    public Guid Id { get; set; }

    public Guid PlantId { get; set; }

    public Guid? PhaseId { get; set; }

    public virtual GrowthPhase? Phase { get; set; }

    public virtual Plant Plant { get; set; } = null!;

    public virtual ICollection<TargetOfPhase> TargetOfPhases { get; set; } = new List<TargetOfPhase>();
}

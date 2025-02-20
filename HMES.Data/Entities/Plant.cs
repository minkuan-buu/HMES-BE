using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class Plant
{
    public Guid Id { get; set; }

    public Guid Name { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<TargetOfPlant> TargetOfPlants { get; set; } = new List<TargetOfPlant>();
}

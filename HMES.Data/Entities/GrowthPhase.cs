using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class GrowthPhase
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual ICollection<DeviceItem> DeviceItems { get; set; } = new List<DeviceItem>();

    public virtual ICollection<PlantOfPhase> PlantOfPhases { get; set; } = new List<PlantOfPhase>();

    public virtual User? User { get; set; }
}

using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class Plant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual ICollection<DeviceItem> DeviceItems { get; set; } = new List<DeviceItem>();

    public virtual ICollection<TargetOfPlant> TargetOfPlants { get; set; } = new List<TargetOfPlant>();
}

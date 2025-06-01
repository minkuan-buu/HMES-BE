using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class DeviceItemDetail
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Serial { get; set; } = null!;
    public string Status { get; set; } = null!;
    public Guid DeviceItemId { get; set; }
    public virtual DeviceItem DeviceItem { get; set; } = null!;
}

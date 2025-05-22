using Microsoft.Identity.Client;

public class DeviceItemDetailResModel
{
    public Guid DeviceItemId { get; set; }
    public string DeviceItemName { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? PlantName { get; set; } = string.Empty;
    public int RefreshCycleHours { get; set; }
    public bool isOnline { get; set; }
    public string Serial { get; set; } = null!;
    public List<PhaseDeviceDetailModel> Phase { get; set; } = new List<PhaseDeviceDetailModel>();
    public IoTResModel IoTData { get; set; } = new IoTResModel();
    public DateTime WarrantyExpiryDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
}

public class PhaseDeviceDetailModel
{
    public Guid Id { get; set; }
    public string PhaseName { get; set; } = null!;
    public bool IsSelected { get; set; }
}

public class HistoryLogIoTResModel
{
    public Guid DeviceItemId { get; set; }
    public string DeviceItemName { get; set; } = null!;
    public List<IoTHistoryResModel>? IoTData { get; set; } = null;
}

public class IoTHistoryResModel
{
    public Guid NutrionId { get; set; }
    public decimal SoluteConcentration { get; set; } = 0;
    public decimal Temperature { get; set; } = 0;
    public decimal Ph { get; set; } = 0;
    public decimal WaterLevel { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
}

public class IoTResModel
{
    public decimal SoluteConcentration { get; set; } = 0;
    public decimal Temperature { get; set; } = 0;
    public decimal Ph { get; set; } = 0;
    public decimal WaterLevel { get; set; } = 0;
}

public class IoTToken
{
    public string Token { get; set; } = null!;
}
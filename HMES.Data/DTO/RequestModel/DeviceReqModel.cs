using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMES.Data.DTO.RequestModel
{
    class DeviceReqModel
    {
    }

    public class DeviceCreateReqModel
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public IFormFile? Attachment { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class DeviceUpdateReqModel
    {
        public string? Name { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public IFormFile? Attachment { get; set; } = null!;
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
    }

    public class SetPlantReqModel
    {
        public Guid DeviceItemId { get; set; }
        public Guid PlantId { get; set; }
    }
    public class SetPhaseReqModel
    {
        public Guid DeviceItemId { get; set; }
        public Guid phaseId { get; set; }
    }

    public class SetValueReqModel
    {
        public Guid DeviceItemId { get; set; }
        public Guid? PhaseId { get; set; }
        public List<TargetReqModel> Values { get; set; } = null!;
    }

    public class UpdateRefreshCycleHoursReqModel
    {
        public int RefreshCycleHours { get; set; }
    }

    public class UpdateLogIoT
    {
        public decimal Temperature { get; set; } = 0;
        public decimal SoluteConcentration { get; set; } = 0;
        public decimal Ph { get; set; } = 0;
        public decimal WaterLevel { get; set; } = 0;
    }

    public class UpdateNameDeviceItem
    {
        public string DeviceItemName { get; set; } = null!;
    }

    public class DeviceActveReqModel
    {
        public Guid DeviceItemId { get; set; }
        public bool IsReconnect { get; set; } = false;
    }
}

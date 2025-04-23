using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMES.Data.DTO.ResponseModel
{
    class DeviceResModel
    {
    }

    public class DeviceCreateResModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Attachment { get; set; }

        public string Serial { get; set; } = null!;

        public decimal Price { get; set; }

    }

    public class DeviceDetailResModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string? Attachment { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

    }

    public class ListDeviceDetailResModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string? Attachment { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

    }

    public class ListActiveDeviceResModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string? PlantName { get; set; }

        public bool IsActive { get; set; }

        public bool IsOnline { get; set; }

        public string Status { get; set; } = null!;

        public DateTime WarrantyExpiryDate { get; set; }

    }

    public class ListMyDeviceResModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public string Status { get; set; } = null!;
    }
}

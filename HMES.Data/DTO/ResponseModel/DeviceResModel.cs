using Microsoft.AspNetCore.Http;
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

        public Guid UserId { get; set; }

        public string Name { get; set; } = null!;

        public string? Attachment { get; set; }

        public string Status { get; set; } = null!;

        public bool IsActive { get; set; }

        public bool IsOnline { get; set; }

        public string Serial { get; set; } = null!;

        public decimal Price { get; set; }

        public DateTime? WarrantyExpiryDate { get; set; }
    }

    public class ListDeviceDetailResModel
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Name { get; set; } = null!;

        public string? Attachment { get; set; }

        public string Status { get; set; } = null!;

        public bool IsActive { get; set; }

        public bool IsOnline { get; set; }

        public string Serial { get; set; } = null!;

        public decimal Price { get; set; }

        public DateTime? WarrantyExpiryDate { get; set; }
    }
}

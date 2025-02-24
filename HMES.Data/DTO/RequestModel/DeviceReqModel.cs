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
        public IFormFile? Attachment { get; set; } = null!;
        public decimal Price { get; set; }
    }
}

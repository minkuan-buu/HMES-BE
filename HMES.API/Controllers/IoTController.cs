using HMES.Business.Services.DeviceItemServices;
using HMES.Business.Services.DeviceServices;
using HMES.Business.Services.UserServices;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers
{
        [Route("api/iot")]
        [ApiController]
        public class IoTController : ControllerBase
        {
                private readonly IDeviceServices _deviceServices;
                private readonly IDeviceItemServices _deviceItemServices;

                public IoTController(IDeviceServices deviceServices, IDeviceItemServices deviceItemServices)
                {
                        _deviceServices = deviceServices;
                        _deviceItemServices = deviceItemServices;
                }

                [HttpPost("{id}")]
                [Authorize(AuthenticationSchemes = "HMESIoTAuthentication")]
                public async Task<IActionResult> UpdateLogFromIoT(Guid id, [FromBody] UpdateLogIoT log)
                {
                        var result = await _deviceItemServices.UpdateLog(log, id);
                        return Ok(result);
                }
        }
}

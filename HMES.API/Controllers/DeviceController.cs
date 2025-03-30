using HMES.Business.Services.DeviceServices;
using HMES.Business.Services.UserServices;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers
{
        [Route("api/device")]
        [ApiController]
        public class DeviceController : ControllerBase
        {
                private readonly IDeviceServices _deviceServices;

                public DeviceController(IDeviceServices deviceServices)
                {
                        _deviceServices = deviceServices;
                }

                [HttpPost]
                [Authorize(AuthenticationSchemes = "HMESAuthentication")]
                public async Task<IActionResult> CreateDevice([FromForm] DeviceCreateReqModel device)
                {
                        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                        var result = await _deviceServices.CreateDevices(device, token);
                        return Ok(result);
                }

                [HttpGet("{Id}")]
                public async Task<IActionResult> GetDeviceDetailById(Guid Id)
                {
                        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                        var result = await _deviceServices.GetDeviceDetailById(Id);
                        return Ok(result);
                }

                // [HttpDelete("{Id}")]
                // [Authorize(AuthenticationSchemes = "HMESAuthentication")]
                // public async Task<IActionResult> DeleteDeviceById(Guid Id)
                // {
                //         var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                //         var result = await _deviceServices.DeleteDeviceById(Id, token);
                //         return Ok(result);
                // }

                [HttpGet]
                public async Task<IActionResult> GetListDevice()
                {
                        var result = await _deviceServices.GetListDevice();
                        return Ok(result);
                }

    }
}

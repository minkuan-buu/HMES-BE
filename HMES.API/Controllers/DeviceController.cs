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
        [Route("api/device")]
        [ApiController]
        public class DeviceController : ControllerBase
        {
                private readonly IDeviceServices _deviceServices;
                private readonly IDeviceItemServices _deviceItemServices;

                public DeviceController(IDeviceServices deviceServices, IDeviceItemServices deviceItemServices)
                {
                        _deviceServices = deviceServices;
                        _deviceItemServices = deviceItemServices;
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

                [HttpPost("active")]
                [Authorize(AuthenticationSchemes = "HMESAuthentication")]
                public async Task<IActionResult> ActiveDevice([FromBody] Guid Id)
                {
                        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                        var result = await _deviceServices.ActiveDevice(token, Id);
                        return Ok(result);
                }
                [HttpPut("set-plant")]
                [Authorize(AuthenticationSchemes = "HMESAuthentication")]
                public async Task<IActionResult> SetPlantForDeviceDevice([FromBody] SetPlantReqModel model)
                {
                        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                        var result = await _deviceItemServices.SetPlantForDevice(model.DeviceItemId, model.PlantId, token);
                        return Ok(result);
                }
                

        }
}

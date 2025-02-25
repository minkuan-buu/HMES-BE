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

        [HttpPost("create-device")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> CreateDevice([FromForm] DeviceCreateReqModel device)
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var result = await _deviceServices.CreateDevices(new List<DeviceCreateReqModel> { device }, token);
                return Ok(result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("device/{Id}")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> GetDeviceDetailById(Guid Id, string token)
        {
            try
            {
                var result = await _deviceServices.GetDeviceDetailById(Id, token);
                return Ok(result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

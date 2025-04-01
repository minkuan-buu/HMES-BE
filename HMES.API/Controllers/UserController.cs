using HMES.Business.Services.UserServices;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HMES.Data.DTO.Custom;
using Microsoft.AspNetCore.Authorization;
using HMES.Business.Services.DeviceServices;
using HMES.Business.Services.DeviceItemServices;

namespace HMES.API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly IDeviceServices _deviceServices;
        private readonly IDeviceItemServices _deviceItemServices;

        public UserController(IUserServices userServices, IDeviceServices deviceServices, IDeviceItemServices deviceItemServices)
        {
            _userServices = userServices;
            _deviceServices = deviceServices;
            _deviceItemServices = deviceItemServices;
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> Profile()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _userServices.Profile(token);
            return Ok(result);
        }

        [HttpGet("technicians")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
        public async Task<IActionResult> GetTechnicians()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _userServices.GetTechnicians();
            return Ok(result);
        }
        
        [HttpGet("staffs/{role}")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> GetStaffs(
            string role
            )
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _userServices.GetStaffsBaseOnRole(token, role);
            return Ok(result);
        }

        [HttpPut("me")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateReqModel UserReqModel)
        {
            var Token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _userServices.Update(UserReqModel, Token);
            return Ok(result);
        }

        // [HttpPost("reset-password")]
        // [Authorize(AuthenticationSchemes = "MeowWoofAuthentication")]
        // public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordReqModel User)
        // {
        //     try
        //     {
        //         string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
        //         var Result = await _userServices.ResetPassword(User, token);
        //         return Ok(Result);
        //     }
        //     catch (CustomException ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        [HttpGet("me/devices")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> GetListActiveDeviceByUserId()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _deviceServices.GetListActiveDeviceByUserId(token);
            return Ok(result);
        }

        [HttpGet("me/devices/{Id}")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> GetDeviceItem([FromQuery] Guid Id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _deviceItemServices.GetDeviceItemDetailById(Id, token);
            return Ok(result);
        }
    }
}
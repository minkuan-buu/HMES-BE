using HMES.Business.Services.UserServices;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HMES.Data.DTO.Custom;
using Microsoft.AspNetCore.Authorization;

namespace HMES.API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;

        public UserController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> Profile()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _userServices.Profile(token);
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
    }
}
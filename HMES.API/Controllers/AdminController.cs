using HMES.Business.Services.UserServices;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HMES.Data.DTO.Custom;
using Microsoft.AspNetCore.Authorization;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Enums;

namespace HMES.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IUserServices _userServices;

        public AdminController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpPost("mod")]
        public async Task<IActionResult> Login([FromBody] CreateModUserModel UserReq)
        {
            var result = await _userServices.CreateModUser(UserReq);
            return Ok(result);
        }
        
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? keyword,
            [FromQuery] AccountStatusEnums? status, [FromQuery] RoleEnums? role, [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var token = Request.Headers.Authorization.ToString().Split(" ")[1];

            var result = await _userServices.GetUsers(token, keyword, role.ToString(), status.ToString(), pageIndex,
                pageSize);
            return Ok(result);
        }
    }
}
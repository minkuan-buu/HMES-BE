using HMES.Business.Services.UserServices;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HMES.Data.DTO.Custom;
using Microsoft.AspNetCore.Authorization;
using HMES.Data.DTO.ResponseModel;

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
    }
}
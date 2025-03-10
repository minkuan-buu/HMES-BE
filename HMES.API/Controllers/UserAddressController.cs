using HMES.Business.Services.UserAddressServices;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers
{
    [Route("api/useraddress")]
    [ApiController]
     public class UserAddressController : ControllerBase
    {
        private readonly IUserAddressServices _userAddressServices;

        public UserAddressController(IUserAddressServices userAddressServices)
        {
            _userAddressServices = userAddressServices;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> CreateDevice([FromBody] UserAddressCreateReqModel userAddressReq)
        {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var result = await _userAddressServices.CreateUserAddress(userAddressReq, token);
                return Ok(result);
        }
    }
}

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
        public async Task<IActionResult> CreateUserAddress([FromBody] UserAddressCreateReqModel userAddressReq)
        {
                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var result = await _userAddressServices.CreateUserAddress(userAddressReq, token);
                return Ok(result);
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> UpdateUserAddress(Guid id, [FromBody] UserAddressUpdateReqModel userAddressReq)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _userAddressServices.UpdateUserAddress(id, userAddressReq, token);
            return Ok(result);
        }
        
        [HttpDelete]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> DeleteUserAddress(Guid id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _userAddressServices.DeleteUserAddress(id, token);
            return Ok(result);
        }
        
        [HttpGet]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> GetUserAddress()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _userAddressServices.GetUserAddress(token);
            return Ok(result);
        }
        
        [HttpPut("address/default/{id}")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> SetDefaultUserAddress(Guid id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _userAddressServices.SetDefaultUserAddress(id, token);
            return Ok(result);
        }
    }
}

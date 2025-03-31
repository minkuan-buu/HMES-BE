using HMES.Business.Services.UserServices;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HMES.Data.DTO.Custom;
using Microsoft.AspNetCore.Authorization;
using HMES.Data.DTO.ResponseModel;
using System.Net;

namespace HMES.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserServices _userServices;

        public AuthController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginReqModel User)
        {
            var result = await _userServices.Login(User);
            Response.Cookies.Append("DeviceId", result.Response.Data.Auth.DeviceId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMonths(6)
            });

            Response.Cookies.Append("RefreshToken", result.Response.Data.Auth.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMonths(6)
            });

            var FinalReturn = new ResultModel<DataResultModel<UserFinalLoginResModel>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new DataResultModel<UserFinalLoginResModel>
                {
                    Data = new UserFinalLoginResModel
                    {
                        Id = result.Response.Data.Id,
                        Name = result.Response.Data.Name,
                        Email = result.Response.Data.Email,
                        Phone = result.Response.Data.Phone,
                        Role = result.Response.Data.Role,
                        Status = result.Response.Data.Status,
                        Attachment = result.Response.Data.Attachment,
                        Auth = new UserFinalAuthResModel
                        {
                            Token = result.Response.Data.Auth.Token,
                        }
                    }
                }
            };

            return Ok(FinalReturn);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterReqModel newUser)
        {
            var result = await _userServices.Register(newUser);
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> Logout()
        {
            var DeviceId = Request.Cookies["DeviceId"];
            if (DeviceId == null)
            {
                throw new CustomException("DeviceId cookie is missing.");
            }
            var result = await _userServices.Logout(Guid.Parse(DeviceId));

            Response.Cookies.Delete("DeviceId", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
            Response.Cookies.Delete("RefreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
            return Ok(result);
        }

        [HttpPost("me/change-password")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordReqModel UserReqModel)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _userServices.ChangePassword(UserReqModel, token);
            return Ok(result);
        }

        [HttpPost("me/reset-password")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordReqModel User)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var Result = await _userServices.ResetPassword(User, token);
            return Ok(Result);
        }
    }
}
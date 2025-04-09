using HMES.Data.Repositories.UserRepositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Microsoft.IdentityModel.Tokens;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Services.UserServices;
using HMES.Business.Services.DeviceItemServices;
using System.Security.Claims;

namespace HMES.API.Middleware
{
    public class IoTAuthorizeMiddleware : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IDeviceItemServices _deviceItemServices;

        public IoTAuthorizeMiddleware(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock systemClock, IDeviceItemServices deviceItemServices)
            : base(options, logger, encoder, systemClock)
        {
            _deviceItemServices = deviceItemServices;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var DeviceId = Context.Request.Headers["X-DeviceItemId"];
            var Token = Context.Request.Headers["X-Token"];
            if (string.IsNullOrEmpty(DeviceId) || string.IsNullOrEmpty(Token))
            {
                return AuthenticateResult.Fail("DeviceId or Token is missing.");
            }

            try
            {
                var deviceItemId = Guid.Parse(DeviceId);
                var deviceItem = await _deviceItemServices.GetDeviceItemById(deviceItemId);
                if (deviceItem == null)
                {
                    return AuthenticateResult.Fail("Device item not found.");
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.AuthenticationMethod, "APIKey"),
                    new Claim("DeviceId", deviceItem.Id.ToString()),
                    new Claim("SerialNumber", deviceItem.Serial),
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (SecurityTokenException ex)
            {
                return AuthenticateResult.Fail("Invalid token: " + ex.Message);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail("Authentication failed: " + ex.Message);
            }
        }
    }
}

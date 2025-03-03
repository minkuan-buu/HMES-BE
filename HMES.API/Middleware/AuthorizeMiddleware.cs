using HMES.Data.Enums;
using HMES.Data.Repositories.UserRepositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using HMES.Data.Repositories.UserTokenRepositories;
using HMES.Business.Services.UserTokenServices;
using HMES.Data.Entities;
using HMES.Data.DTO.Custom;

namespace HMES.API.Middleware
{
    public class AuthorizeMiddleware : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserRepositories _userRepositories;
        private readonly IUserTokenServices _userTokenServices;
        private static string Key = "TestingIssuerSigningKeyPTEducationMS@123";
        private static string Issuser = "TestingJWTIssuerSigningPTEducationMS@123";

        public AuthorizeMiddleware(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock systemClock, IUserRepositories userRepositories, IUserTokenServices userTokenServices)
            : base(options, logger, encoder, systemClock)
        {
            _userTokenServices = userTokenServices;
            _userRepositories = userRepositories;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var requestPath = Context.Request.Path;

            // Allow the login endpoint to be bypassed
            if (requestPath.StartsWithSegments("/api/auth/login") || requestPath.StartsWithSegments("/api/auth/register"))
            {
                return AuthenticateResult.NoResult(); // Cho phép request đi qua mà không xác thực
            }

            // Get the Authorization header
            string authorizationHeader = Request.Headers["Authorization"].FirstOrDefault() ?? string.Empty;

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return AuthenticateResult.Fail("Authorization header is missing or invalid.");
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            // Validate the JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("TestingIssuerSigningKeyPTEducationMS@123"); // Use your JWT signing key here

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = false, // Tắt kiểm tra thời gian sống để tránh bị throw exception
                    ValidIssuer = "TestingJWTIssuerSigningPTEducationMS@123",
                    ValidAudience = "TestingJWTIssuerSigningPTEducationMS@123",
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                
                // Giải mã token, không kiểm tra thời gian sống
                var claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                var identity = claimsPrincipal.Identity as ClaimsIdentity;
                if (identity == null || !identity.IsAuthenticated)
                {
                    return AuthenticateResult.Fail("Unauthorized");
                }

                // Kiểm tra yêu cầu đặt lại mật khẩu
                var typeClaim = identity.FindFirst("type")?.Value;
                if (requestPath.StartsWithSegments("/api/auth/me/reset-password"))
                {
                    if (typeClaim != "reset")
                    {
                        return AuthenticateResult.Fail("Invalid token for reset-password.");
                    } else if (validatedToken.ValidTo < DateTime.UtcNow)
                    {
                        return AuthenticateResult.Fail("Token is expired.");
                    }
                }
                else if (typeClaim == "reset")
                {
                    return AuthenticateResult.Fail("Invalid token for reset-password.");
                } else {
                    var DeviceId = Context.Request.Cookies["DeviceId"];
                
                    var RefreshToken = Context.Request.Cookies["RefreshToken"];
                    
                    if (string.IsNullOrEmpty(RefreshToken))
                        throw new CustomException("RefreshToken is missing.");
                    
                    if (string.IsNullOrEmpty(DeviceId))
                        throw new CustomException("DeviceId is missing.");

                    if (!Guid.TryParse(DeviceId, out var deviceGuid))
                        throw new CustomException("Invalid DeviceId format.");

                    var UserToken = await _userTokenServices.GetUserToken(deviceGuid);
                    
                    if (UserToken == null || UserToken.RefreshToken != RefreshToken)
                        throw new CustomException("RefreshToken is invalid.");

                    if(token != UserToken.AccesToken)
                        return AuthenticateResult.Fail("AccessToken is invalid.");
                    
                    // Kiểm tra nếu token đã hết hạn (tự handle)
                    if (validatedToken.ValidTo < DateTime.UtcNow)
                    {
                        return await HandleExpiredTokenAsync(UserToken);
                    }

                    // Kiểm tra user trong DB
                    var userIdClaim = identity.FindFirst("userid")?.Value;
                    if (Guid.TryParse(userIdClaim, out var userId))
                    {
                        var user = await _userRepositories.GetSingle(u => u.Id == userId);
                        if (user == null || user.Status.Equals(AccountStatusEnums.Inactive.ToString()))
                        {
                            return AuthenticateResult.Fail("User is inactive or not found.");
                        }
                    }

                    // Kiểm tra vai trò
                    var endpointRoles = GetEndpointRoles();
                    var userRoles = identity.Claims
                        .Where(c => c.Type == ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToList();

                    if (endpointRoles.Any() && !userRoles.Any(ur => endpointRoles.Contains(ur)))
                    {
                        throw new CustomException("Access denied"); // Dùng đúng thông báo để middleware xử lý 403
                    }
                }
                var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (SecurityTokenException ex)
            {
                return AuthenticateResult.Fail($"Token validation failed: {ex.Message}");
            }
        }

        private async Task<AuthenticateResult> HandleExpiredTokenAsync(UserToken UserToken)
        {
            var lastUpdated = UserToken.UpdatedAt ?? UserToken.CreatedAt;

            if (DateTime.UtcNow - lastUpdated > TimeSpan.FromDays(180))
            {
                return AuthenticateResult.Fail("RefreshToken is expired.");
            }

            var user = await _userRepositories.GetSingle(u => u.Id == UserToken.UserId);
            if (user == null || user.Status.Equals(AccountStatusEnums.Inactive.ToString()))
            {
                return AuthenticateResult.Fail("User is inactive or not found.");
            }

            // Tạo token mới
            var newToken = GenerateJwtToken(user);
            UserToken.AccesToken = newToken;
            UserToken.UpdatedAt = DateTime.UtcNow;
            await _userTokenServices.UpdateUserToken(UserToken);

            // Lưu token vào response header để client lấy về
            Context.Response.Headers["New-Access-Token"] = newToken;

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("userid", user.Id.ToString()),
                new Claim("email", user.Email)
            }, Scheme.Name));

            var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("userid", user.Id.ToString()),
                new Claim("email", user.Email),
            };

            var token = new JwtSecurityToken(
                issuer: Issuser,
                audience: Issuser,
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private List<string> GetEndpointRoles()
        {
            var endpoint = Context.GetEndpoint();
            if (endpoint == null)
            {
                return new List<string>();
            }

            var authorizeAttributes = endpoint.Metadata.GetOrderedMetadata<AuthorizeAttribute>();
            var roles = new List<string>();

            foreach (var attribute in authorizeAttributes)
            {
                if (!string.IsNullOrEmpty(attribute.Roles))
                {
                    roles.AddRange(attribute.Roles.Split(',').Select(r => r.Trim()));
                }
            }

            return roles;
        }
    }
}

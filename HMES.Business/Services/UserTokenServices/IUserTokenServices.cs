using HMES.Data.Entities;

namespace HMES.Business.Services.UserTokenServices;

public interface IUserTokenServices
{
    Task<UserToken> GetUserToken(Guid DeviceId);
    Task UpdateUserToken(UserToken userToken);
}
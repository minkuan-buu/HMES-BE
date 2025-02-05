using HMES.Data.Entities;

namespace HMES.Business.Services.UserServices;

public interface IUserServices
{
    Task<User> GetUser();
}
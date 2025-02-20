using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;

namespace HMES.Business.Services.UserServices;

public interface IUserServices
{
    Task<User> GetUser();
    Task<UserLoginResModel> Login(UserLoginReqModel UserReqModel);
}
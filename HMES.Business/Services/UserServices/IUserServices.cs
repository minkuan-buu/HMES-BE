using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using MeowWoofSocial.Data.DTO.ResponseModel;

namespace HMES.Business.Services.UserServices;

public interface IUserServices
{
    Task<User> GetUser();
    Task<ResultModel<DataResultModel<UserLoginResModel>>> Login(UserLoginReqModel UserReqModel);
    Task<ResultModel<MessageResultModel>> Register(UserRegisterReqModel UserReqModel);
}
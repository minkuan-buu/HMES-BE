using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;

namespace HMES.Business.Services.UserServices;

public interface IUserServices
{
    Task<ResultModel<ListDataResultModel<UserProfileResModel>>> GetTechnicians();
    Task<ResultModel<DataResultModel<UserLoginResModel>>> Login(UserLoginReqModel UserReqModel);
    Task<ResultModel<MessageResultModel>> Register(UserRegisterReqModel UserReqModel);
    Task<ResultModel<MessageResultModel>> Logout(Guid DeviceId);
    Task<ResultModel<MessageResultModel>> ChangePassword(UserChangePasswordReqModel UserReqModel, string Token);
    Task<ResultModel<MessageResultModel>> Update(UserUpdateReqModel UserReqModel, string Token);
    Task<ResultModel<DataResultModel<UserProfileResModel>>> Profile(string Token);
    Task<ResultModel<MessageResultModel>> ResetPassword(UserResetPasswordReqModel ReqModel, string token);
}
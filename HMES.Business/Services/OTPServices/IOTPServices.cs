using HMES.Data.DTO.ResponseModel;

namespace HMES.Business.Services.OTPServices
{
    public interface IOTPServices
    {
        Task<ResultModel<MessageResultModel>> SendOTP(string Email);
        Task<ResultModel<DataResultModel<UserTemp>>> VerifyOTPCode(string Email, string OTPCode);
    }
}
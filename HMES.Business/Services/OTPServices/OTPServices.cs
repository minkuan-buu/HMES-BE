using System.Net;
using HMES.Business.Utilities.Email;
using HMES.Business.Utilities.Authentication;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.OTPRepositories;
using HMES.Data.Repositories.UserRepositories;

namespace HMES.Business.Services.OTPServices
{
    public class OTPServices : IOTPServices
    {
        private readonly IEmail _email;
        private readonly IOTPRepositories _OtpRepositories;
        private readonly IUserRepositories _UserRepositories;

        public OTPServices(IEmail email, IOTPRepositories OtpRepositories, IUserRepositories UserRepositories)
        {
            _email = email;
            _OtpRepositories = OtpRepositories;
            _UserRepositories = UserRepositories;
        }

        private string CreateOTPCode()
        {
            Random rnd = new();
            return rnd.Next(100000, 999999).ToString();
        }

        public async Task<ResultModel<MessageResultModel>> SendOTP(string Email)
        {
            var User = await _UserRepositories.GetSingle(x => x.Email.Equals(Email), includeProperties: "Otps");
            if (User == null)
            {
                throw new CustomException("User not found!");
            }
            var getActiveOtp = User.Otps.Where(x => x.Status.Equals(GeneralStatusEnums.Active.ToString())).ToList();
            foreach (var otp in getActiveOtp)
            {
                if ((otp.ExpiredDate.Value - DateTime.Now).TotalMinutes > 8)
                {
                    throw new CustomException("Can not send OTP right now!");
                }
                else
                {
                    otp.Status = GeneralStatusEnums.Inactive.ToString();
                }
            }
            await _OtpRepositories.UpdateRange(getActiveOtp);
            string OTPCode = CreateOTPCode();
            string FilePath = "./ResetPassword.html";
            string Html = File.ReadAllText(FilePath);
            for (int i = 0; i < OTPCode.Length; i++)
            {
                Html = Html.Replace($"[{i}]", OTPCode[i].ToString());
            }
            List<EmailReqModel> ListEmailReq = new()
            {
                new EmailReqModel { Email = Email, HtmlContent = Html },
            };
            Otp Otp = new()
            {
                Id = Guid.NewGuid(),
                UserId = User.Id,
                Code = OTPCode,
                ExpiredDate = DateTime.Now.AddMinutes(10),
                Status = GeneralStatusEnums.Active.ToString(),
                IsUsed = false
            };
            await _OtpRepositories.Insert(Otp);
            await _email.SendEmail("Đặt lại mật khẩu", ListEmailReq);
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response =
                new MessageResultModel()
                {
                    Message = "Ok"
                }
            };
        }

        public async Task<ResultModel<DataResultModel<UserTemp>>> VerifyOTPCode(string Email, string OTPCode)
        {
            try
            {
                var User = await _UserRepositories.GetSingle(x => x.Email.Equals(Email), includeProperties: "Otps");
                if (User == null)
                {
                    throw new CustomException("User not found");
                }
                var GetOTP = User.Otps.FirstOrDefault(x => x.Code.Equals(OTPCode) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
                if (GetOTP != null)
                {
                    if ((DateTime.Now - GetOTP.ExpiredDate.Value).TotalMinutes > 10 || GetOTP.IsUsed)
                    {
                        throw new CustomException("The OTP is expired!");
                    }
                    GetOTP.IsUsed = true;
                    GetOTP.Status = GeneralStatusEnums.Inactive.ToString();
                    await _OtpRepositories.Update(GetOTP);
                    User.Status = AccountStatusEnums.ResetPassword.ToString();
                    await _UserRepositories.Update(User);
                    var Result = new UserTemp()
                    {
                        TempToken = Authentication.GenerateTempJWT(Email)
                    };
                    return new ResultModel<DataResultModel<UserTemp>>()
                    {
                        StatusCodes = (int)HttpStatusCode.OK,
                        Response = new DataResultModel<UserTemp>()
                        {
                            Data = Result
                        }
                    };
                }
                else
                {
                    throw new CustomException("The OTP is invalid!");
                }
            }
            catch (Exception e)
            {
                throw new CustomException(e.Message);
            }
        }
    }
}
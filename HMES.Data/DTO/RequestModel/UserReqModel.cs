using System.ComponentModel.DataAnnotations;
using HMES.Data.DTO.Custom;

namespace HMES.Data.DTO.RequestModel
{
    public class UserReqModel
    {
    }


    public class UserLoginReqModel
    {
        [CustomEmailValidate]
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class UserRegisterReqModel
    {
        public string Name { get; set; } = null!;
        [CustomEmailValidate]
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class UserChangePasswordReqModel
    {
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }

    public class UserUpdateReqModel
    {
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }

    public class UserResetPasswordReqModel
    {
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }

    public class UserVerifyOTPReqModel
    {
        public string Email { get; set; } = null!;
        public string OTPCode { get; set; } = null!;
    }
}
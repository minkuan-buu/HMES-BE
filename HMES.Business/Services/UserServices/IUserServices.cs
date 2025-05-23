﻿using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;

namespace HMES.Business.Services.UserServices;

public interface IUserServices
{
    Task<ResultModel<ListDataResultModel<UserProfileResModel>>> GetTechnicians();
    Task<ResultModel<ListDataResultModel<StaffBriefInfoModel>>> GetStaffsBaseOnRole(string token, string role);
    Task<ResultModel<DataResultModel<UserLoginResModel>>> Login(UserLoginReqModel UserReqModel);
    Task<ResultModel<MessageResultModel>> Register(UserRegisterReqModel UserReqModel);
    Task<ResultModel<MessageResultModel>> Logout(Guid DeviceId);
    Task<ResultModel<MessageResultModel>> ChangePassword(UserChangePasswordReqModel UserReqModel, string Token);
    Task<ResultModel<MessageResultModel>> Update(UserUpdateReqModel UserReqModel, string Token);
    Task<ResultModel<DataResultModel<UserProfileResModel>>> Profile(string Token);
    Task<ResultModel<MessageResultModel>> ResetPassword(UserResetPasswordReqModel ReqModel, string token);
    Task<ResultModel<MessageResultModel>> CreateModUser(CreateModUserModel ReqModel);
    Task<ResultModel<ListDataResultModel<UserProfileResModel>>> GetUsers(string token, string? keyword, string? role, string? status, int pageIndex, int pageSize);
    Task<User> GetUserById(Guid id);
    Task<int> GetUserCount();
}
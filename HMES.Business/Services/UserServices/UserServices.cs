using HMES.Data.DTO.Custom;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Repositories.UserRepositories;
using HMES.Business.Utilities.Authentication;
using HMES.Data.DTO.RequestModel;
using AutoMapper;
using System.Net;
using System.Security.Claims;
using HMES.Data.Repositories.UserTokenRepositories;
using HMES.Data.Enums;
using HMES.Business.Utilities.Converter;
using HMES.Business.Utilities.Email;
namespace HMES.Business.Services.UserServices;

public class UserServices : IUserServices
{
    private readonly IUserRepositories _userRepositories;
    private readonly IUserTokenRepositories _userTokenRepositories;
    private readonly IEmail _email;
    private readonly IMapper _mapper;

    public UserServices(IUserRepositories userRepositories, IMapper mapper, IUserTokenRepositories userTokenRepositories, IEmail email)
    {
        _email = email;
        _userRepositories = userRepositories;
        _mapper = mapper;
        _userTokenRepositories = userTokenRepositories;
    }

    public async Task<User> GetUser()
    {
        var list = await _userRepositories.GetList();
        return list.FirstOrDefault();
    }

    public async Task<ResultModel<ListDataResultModel<StaffBriefInfoModel>>> GetStaffsBaseOnRole(string token, string role)
    {
        var userRole = Authentication.DecodeToken(token, ClaimsIdentity.DefaultRoleClaimType);
        Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));

        IEnumerable<User> staffList;

        if (userRole == RoleEnums.Admin.ToString())
        {
            staffList = await _userRepositories.GetList(x => x.Role.Equals(role) && x.Status.Equals(GeneralStatusEnums.Active.ToString()) && x.Id != userId);
        }
        else
        {
            staffList = await _userRepositories.GetList(x => x.Role.Equals(userRole) && x.Status.Equals(GeneralStatusEnums.Active.ToString()) && x.Id != userId);
        }

        var result = _mapper.Map<List<StaffBriefInfoModel>>(staffList);
        return new ResultModel<ListDataResultModel<StaffBriefInfoModel>>()
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new ListDataResultModel<StaffBriefInfoModel>()
            {
                Data = result
            }
        };
    }

    public async Task<ResultModel<DataResultModel<UserLoginResModel>>> Login(UserLoginReqModel UserReqModel)
    {
        var user = await _userRepositories.GetUserByEmail(UserReqModel.Email);
        if (user == null)
        {
            throw new CustomException("User not found");
        }
        var checkPassword = Authentication.VerifyPasswordHashed(UserReqModel.Password, user.Salt, user.Password);
        if (!checkPassword)
        {
            throw new CustomException("Password is incorrect");
        }
        UserLoginResModel Result = _mapper.Map<UserLoginResModel>(user);
        UserToken NewUserToken = new UserToken()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccesToken = Result.Auth.Token,
            RefreshToken = Result.Auth.RefreshToken,
            CreatedAt = DateTime.Now
        };
        Result.Auth.DeviceId = NewUserToken.Id;
        await _userTokenRepositories.Insert(NewUserToken);
        return new ResultModel<DataResultModel<UserLoginResModel>>()
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new DataResultModel<UserLoginResModel>()
            {
                Data = Result
            }
        };
    }

    public async Task<ResultModel<MessageResultModel>> Register(UserRegisterReqModel UserReqModel)
    {
        var User = await _userRepositories.GetUserByEmail(UserReqModel.Email);
        if (User != null)
        {
            throw new CustomException("This email is existed!");
        }
        CreateHashPasswordModel PasswordSet = Authentication.CreateHashPassword(UserReqModel.Password);
        User NewUser = _mapper.Map<User>(UserReqModel);
        NewUser.Password = PasswordSet.HashedPassword;
        NewUser.Salt = PasswordSet.Salt;
        await _userRepositories.Insert(NewUser);
        return new ResultModel<MessageResultModel>()
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel()
            {
                Message = "Account is created!"
            }
        };
    }

    public async Task<ResultModel<DataResultModel<UserProfileResModel>>> Profile(string Token)
    {
        var userId = Authentication.DecodeToken(Token, "userid");
        var user = await _userRepositories.GetSingle(x => x.Id.Equals(Guid.Parse(userId)));
        if (user == null)
        {
            throw new CustomException("User not found");
        }
        UserProfileResModel Result = _mapper.Map<UserProfileResModel>(user);
        return new ResultModel<DataResultModel<UserProfileResModel>>()
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new DataResultModel<UserProfileResModel>()
            {
                Data = Result
            }
        };
    }

    public async Task<ResultModel<MessageResultModel>> Logout(Guid DeviceId)
    {
        var MessageReturn = "Logout success! ";
        var UserToken = await _userTokenRepositories.GetSingle(x => x.Id == DeviceId);
        if (UserToken == null)
        {
            MessageReturn += "Warning: DeviceId is not found!";
        }
        else await _userTokenRepositories.Delete(UserToken);
        return new ResultModel<MessageResultModel>()
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel()
            {
                Message = MessageReturn
            }
        };
    }

    public async Task<ResultModel<MessageResultModel>> ChangePassword(UserChangePasswordReqModel UserReqModel, string Token)
    {
        var UserId = Authentication.DecodeToken(Token, "userid");
        var User = await _userRepositories.GetSingle(x => x.Id.Equals(Guid.Parse(UserId)));
        if (User == null)
        {
            throw new CustomException("User not found");
        }
        var CheckPassword = Authentication.VerifyPasswordHashed(UserReqModel.OldPassword, User.Salt, User.Password);
        if (!CheckPassword)
        {
            throw new CustomException("Old password is incorrect");
        }
        if (UserReqModel.NewPassword != UserReqModel.ConfirmPassword)
        {
            throw new CustomException("New password and confirm password are not matched");
        }
        CreateHashPasswordModel PasswordSet = Authentication.CreateHashPassword(UserReqModel.NewPassword);
        User.Password = PasswordSet.HashedPassword;
        User.Salt = PasswordSet.Salt;
        await _userRepositories.Update(User);
        return new ResultModel<MessageResultModel>()
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel()
            {
                Message = "Password is changed!"
            }
        };
    }

    public async Task<ResultModel<MessageResultModel>> Update(UserUpdateReqModel UserReqModel, string Token)
    {
        var UserId = Guid.Parse(Authentication.DecodeToken(Token, "userid"));
        var User = await _userRepositories.GetSingle(x => x.Id == UserId);
        if (User == null)
        {
            throw new CustomException("User not found");
        }
        User.Name = TextConvert.ConvertToUnicodeEscape(UserReqModel.Name);
        User.Phone = UserReqModel.Phone;
        User.UpdatedAt = DateTime.Now;
        await _userRepositories.Update(User);
        return new ResultModel<MessageResultModel>()
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel()
            {
                Message = "User is updated!"
            }
        };
    }

    public async Task<ResultModel<MessageResultModel>> ResetPassword(UserResetPasswordReqModel ReqModel, string token)
    {
        try
        {
            var email = Authentication.DecodeToken(token, "email");
            var user = await _userRepositories.GetSingle(x => x.Email.Equals(email));
            if (user == null)
            {
                throw new CustomException("User not found!");
            }
            if (ReqModel.NewPassword != ReqModel.ConfirmPassword)
            {
                throw new CustomException("New password and confirm password is not match!");
            }
            if (ReqModel.NewPassword.Length < 6)
            {
                throw new CustomException("Password must be at least 6 characters!");
            }
            var Auth = Authentication.CreateHashPassword(ReqModel.NewPassword);
            user.Password = Auth.HashedPassword;
            user.Salt = Auth.Salt;
            user.Status = GeneralStatusEnums.Active.ToString();
            await _userRepositories.Update(user);
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new MessageResultModel()
                {
                    Message = "Ok"
                }
            };
        }
        catch (Exception ex)
        {
            throw new CustomException(ex.Message);
        }
    }

    public async Task<ResultModel<ListDataResultModel<UserProfileResModel>>> GetTechnicians()
    {
        var TechniciansList = await _userRepositories.GetList(x => x.Role.Equals(RoleEnums.Technician.ToString()) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
        var Result = _mapper.Map<List<UserProfileResModel>>(TechniciansList);
        return new ResultModel<ListDataResultModel<UserProfileResModel>>()
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new ListDataResultModel<UserProfileResModel>()
            {
                Data = Result
            }
        };
    }

    public async Task<ResultModel<MessageResultModel>> CreateModUser(CreateModUserModel ReqModel)
    {
        var User = await _userRepositories.GetUserByEmail(ReqModel.Email);
        if (User != null)
        {
            throw new CustomException("This email is existed!");
        }
        var GeneratePassword = Authentication.GenerateRandomPassword();
        CreateHashPasswordModel PasswordSet = Authentication.CreateHashPassword(GeneratePassword);
        User NewUser = _mapper.Map<User>(ReqModel);
        NewUser.Password = PasswordSet.HashedPassword;
        NewUser.Salt = PasswordSet.Salt;
        string FilePath = "./Information.html";
        string Html = File.ReadAllText(FilePath);

        Html = Html.Replace("[Role]", NewUser.Role)
                   .Replace("[Email]", NewUser.Email)
                   .Replace("[Password]", GeneratePassword);
        List<EmailReqModel> EmailList = new List<EmailReqModel>()
        {
            new EmailReqModel()
            {
                Email = ReqModel.Email,
                HtmlContent = Html,
            }
        };
        await _userRepositories.Insert(NewUser);
        await _email.SendEmail("[Thông tin tài khoản]", EmailList);
        return new ResultModel<MessageResultModel>()
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel()
            {
                Message = "Account is created!"
            }
        };
    }

    public async Task<ResultModel<ListDataResultModel<UserProfileResModel>>> GetUsers(string token, string? keyword,
        string? role, string? status, int pageIndex, int pageSize)
    {
        var userId = Guid.Parse(Authentication.DecodeToken(token, "userid"));
        if (role == RoleEnums.Admin.ToString())
        {
            role = null;
        }

        var (tickets, totalItems) =
            await _userRepositories.GetAllUsersAsync(keyword, userId, role, status, pageIndex, pageSize);


        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var result = new ListDataResultModel<UserProfileResModel>
        {
            Data = _mapper.Map<List<UserProfileResModel>>(tickets),
            CurrentPage = pageIndex,
            TotalPages = totalPages,
            TotalItems = totalItems,
            PageSize = pageSize
        };

        return new ResultModel<ListDataResultModel<UserProfileResModel>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
    }

    public async Task<User> GetUserById(Guid id)
    {
        return await _userRepositories.GetSingle(x => x.Id == id);
    }
}
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Repositories.UserRepositories;
using HMES.Business.Utilities.Authentication;
using HMES.Data.DTO.RequestModel;
using AutoMapper;
using MeowWoofSocial.Data.DTO.ResponseModel;
using System.Net;
using HMES.Data.Repositories.UserTokenRepositories;
namespace HMES.Business.Services.UserServices;

public class UserServices : IUserServices
{
    private readonly IUserRepositories _userRepositories;
    private readonly IUserTokenRepositories _userTokenRepositories;
    private readonly IMapper _mapper;

    public UserServices(IUserRepositories userRepositories, IMapper mapper, IUserTokenRepositories userTokenRepositories)
    {
        _userRepositories = userRepositories;
        _mapper = mapper;
        _userTokenRepositories = userTokenRepositories;
    }
    
    public async Task<User> GetUser()
    {
        var list = await _userRepositories.GetList();
        return list.FirstOrDefault();
    }

    public async Task<ResultModel<DataResultModel<UserLoginResModel>>> Login(UserLoginReqModel UserReqModel)
    {
        var user = await _userRepositories.GetUserByEmail(UserReqModel.Email);
        if(user == null)
        {
            throw new CustomException("User not found");
        }
        var checkPassword = Authentication.VerifyPasswordHashed(UserReqModel.Password, user.Salt, user.Password);
        if(!checkPassword)
        {
            throw new CustomException("Password is incorrect");
        }
        UserLoginResModel Result = _mapper.Map<UserLoginResModel>(user);
        UserToken NewUserToken = new UserToken()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccesToken = Result.Auth.Token,
            RefreshToken = Result.Auth.RefeshToken,
            CreatedAt = DateTime.Now
        };
        Result.Auth.DeviceId = NewUserToken.Id;
        await _userTokenRepositories.Insert(NewUserToken);
        return new ResultModel<DataResultModel<UserLoginResModel>>(){
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new DataResultModel<UserLoginResModel>(){
                Data = Result
            }
        };
    }

    public async Task<ResultModel<MessageResultModel>> Register(UserRegisterReqModel UserReqModel)
    {
        var User = await _userRepositories.GetUserByEmail(UserReqModel.Email);
        if(User != null) {
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

    public async Task<ResultModel<DataResultModel<UserProfileResModel>>> Profile(string Token){
        var userId = Authentication.DecodeToken(Token,"userid");
        var user = await _userRepositories.GetSingle(x => x.Id.Equals(Guid.Parse(userId)));
        if(user == null){
            throw new CustomException("User not found");
        }
        UserProfileResModel Result = _mapper.Map<UserProfileResModel>(user);
        return new ResultModel<DataResultModel<UserProfileResModel>>(){
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new DataResultModel<UserProfileResModel>(){
                Data = Result
            }
        };
    }

    public async Task<ResultModel<MessageResultModel>> Logout(Guid DeviceId)
    {
        var MessageReturn = "Logout success! ";
        var UserToken = await _userTokenRepositories.GetSingle(x => x.Id == DeviceId);
        if(UserToken == null)
        {
            MessageReturn += "Warning: DeviceId is not found!";
        } else await _userTokenRepositories.Delete(UserToken);
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
        var UserId = Authentication.DecodeToken(Token,"userid");
        var User = await _userRepositories.GetSingle(x => x.Id.Equals(Guid.Parse(UserId)));
        if(User == null){
            throw new CustomException("User not found");
        }
        var CheckPassword = Authentication.VerifyPasswordHashed(UserReqModel.OldPassword, User.Salt, User.Password);
        if(!CheckPassword)
        {
            throw new CustomException("Old password is incorrect");
        }
        if(UserReqModel.NewPassword != UserReqModel.ConfirmPassword)
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
}
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Repositories.UserRepositories;
using HMES.Business.Utilities.Authentication;
using HMES.Data.DTO.RequestModel;
using AutoMapper;
using MeowWoofSocial.Data.DTO.ResponseModel;
using System.Net;
namespace HMES.Business.Services.UserServices;

public class UserServices : IUserServices
{
    private readonly IUserRepositories _userRepositories;
    private readonly IMapper _mapper;

    public UserServices(IUserRepositories userRepositories, IMapper mapper)
    {
        _userRepositories = userRepositories;
        _mapper = mapper;
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
}
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Repositories.UserRepositories;
using HMES.Business.Utilities.Authentication;
using HMES.Data.DTO.RequestModel;
using AutoMapper;
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

    public async Task<UserLoginResModel> Login(UserLoginReqModel UserReqModel)
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
        return Result;
    }
}
using HMES.Data.Entities;
using HMES.Data.Repositories.UserRepositories;

namespace HMES.Business.Services.UserServices;

public class UserServices : IUserServices
{
    private readonly IUserRepositories _userRepositories;

    public UserServices(IUserRepositories userRepositories)
    {
        _userRepositories = userRepositories;
    }
    
    public async Task<User> GetUser()
    {
        var list = await _userRepositories.GetList();
        return list.FirstOrDefault();
    }
}
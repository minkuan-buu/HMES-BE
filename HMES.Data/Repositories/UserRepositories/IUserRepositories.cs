using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMES.Data.Repositories.UserRepositories
{
    public interface IUserRepositories : IGenericRepositories<User>
    {
       Task<User?> GetUserByEmail(string email);
       Task<bool> CheckUserByIdAndRole(Guid id, string role);
    }
}

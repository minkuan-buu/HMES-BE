using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMES.Data.Repositories.UserRepositories
{
    public class UserRepositories : GenericRepositories<User>, IUserRepositories
    {
        public UserRepositories(HmesContext context)
        : base(context)
        {
        }
        
        public async Task<User?> GetUserByEmail(string email){
            return await Context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> CheckUserByIdAndRole(Guid id, string role)
        {
            return await Context.Users.AnyAsync(u => u.Id == id && u.Role == role);
        }
    }
}

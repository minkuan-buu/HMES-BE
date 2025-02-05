using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
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

    }
}

using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMES.Data.Repositories.UserTokenRepositories
{
    public class UserTokenRepositories : GenericRepositories<UserToken>, IUserTokenRepositories
    {
        public UserTokenRepositories(HmesContext context)
        : base(context)
        {
        }
    }
}

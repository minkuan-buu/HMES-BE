using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.UserAddressRepositories
{
    public class UserAddressRepositories : GenericRepositories<UserAddress>, IUserAddressRepositories
    {
        public UserAddressRepositories(HmesContext context) : base(context)
        {
        }
    }
}
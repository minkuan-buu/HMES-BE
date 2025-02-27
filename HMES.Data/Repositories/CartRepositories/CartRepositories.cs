using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.CartRepositories
{
    public class CartRepositories : GenericRepositories<Cart>, ICartRepositories
    {
        public CartRepositories(HmesContext context) : base(context)
        {
        }
    }
}
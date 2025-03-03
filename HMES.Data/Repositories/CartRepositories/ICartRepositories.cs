using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.CartRepositories;

public interface ICartRepositories:IGenericRepositories<Cart>
{
    Task<Cart?> GetCartByUserId(Guid userId);
}

using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.CartRepositories;

public interface ICartItemsRepositories:IGenericRepositories<CartItem>
{
    Task<int> GetTotalItemsInCart(Guid cartId);
    Task<CartItem?> GetItemExist(Guid newCartId, Guid productId);
    Task<bool> DeleteAll(Guid cartId);
    Task<(List<CartItem> Items, int TotalItems)> GetItemsPaging(Guid cartId, int pageIndex, int pageSize);
}
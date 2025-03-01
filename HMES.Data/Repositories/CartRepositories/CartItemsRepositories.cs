using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.CartRepositories;

public class CartItemsRepositories:GenericRepositories<CartItem>,ICartItemsRepositories
{
    public CartItemsRepositories(HmesContext context) : base(context)
    {
    }

    public async Task<int> GetTotalItemsInCart(Guid cartId)
    {
        //count distinct items in cart
        return await Context.CartItems.Where(c => c.CartId == cartId)
            .Select(c => c.ProductId).Distinct()
            .CountAsync();
    }

    public async Task<CartItem?> GetItemExist(Guid newCartId, Guid productId)
    {
        return await Context.CartItems.FirstOrDefaultAsync(c => c.CartId == newCartId && c.ProductId == productId);
    }

    public async Task<bool> DeleteAll(Guid cartId)
    {
        var cartItems = await Context.CartItems.Where(c => c.CartId == cartId).ToListAsync();
        Context.CartItems.RemoveRange(cartItems);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<(List<CartItem> Items, int TotalItems)> GetItemsPaging(Guid cartId, int pageIndex, int pageSize)
    {
        //get cart items paging
        var cartItems = await Context.CartItems
            .Include(c => c.Product)
            .Where(c => c.CartId == cartId)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalItems = Context.CartItems.Count(c => c.CartId == cartId);

        return (cartItems, totalItems);
    }
}
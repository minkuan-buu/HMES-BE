using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.CartRepositories;

public class CartRepositories : GenericRepositories<Cart>,ICartRepositories
{
    public CartRepositories(HmesContext context) : base(context)
    {
    }

    public async Task<Cart?> GetCartByUserId(Guid userId)
    {
        return await Context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
    }
}
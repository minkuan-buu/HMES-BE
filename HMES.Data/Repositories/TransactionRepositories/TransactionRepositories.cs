using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.TransactionRepositories
{
    public class TransactionRepositories : GenericRepositories<Transaction>, ITransactionRepositories
    {
        public TransactionRepositories(HmesContext context) : base(context)
        {
        }

        public async Task<List<Transaction>> GetPendingTransaction()
        {
            var fifteenMinutesAgo = DateTime.Now.AddMinutes(-15);

            return await Context.Transactions
                .Include(x => x.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(d => d.Product)
                .Include(x => x.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(d => d.Device)
                .Include(x => x.Order)
                    .ThenInclude(o => o.UserAddress)
                .Where(x => x.Status == TransactionEnums.PENDING.ToString()
                            && x.CreatedAt <= fifteenMinutesAgo)
                .ToListAsync();
        }

    }
}
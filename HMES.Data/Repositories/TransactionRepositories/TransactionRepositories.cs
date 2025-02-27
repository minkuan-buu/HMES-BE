using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.TransactionRepositories
{
    public class TransactionRepositories : GenericRepositories<Transaction>, ITransactionRepositories
    {
        public TransactionRepositories(HmesContext context) : base(context)
        {
        }
    }
}
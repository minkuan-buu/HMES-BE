using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.OrderRepositories
{
    public class OrderRepositories : GenericRepositories<Order>, IOrderRepositories
    {
        public OrderRepositories(HmesContext context) : base(context)
        {
        }
    }
}
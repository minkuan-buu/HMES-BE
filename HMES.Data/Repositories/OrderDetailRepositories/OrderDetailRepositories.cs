using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.OrderDetailRepositories
{
    public class OrderDetailRepositories : GenericRepositories<OrderDetail>, IOrderDetailRepositories
    {
        public OrderDetailRepositories(HmesContext context) : base(context)
        {
        }
    }
}
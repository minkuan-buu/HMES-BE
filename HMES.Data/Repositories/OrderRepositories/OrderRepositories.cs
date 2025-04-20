using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.OrderRepositories
{
    public class OrderRepositories : GenericRepositories<Order>, IOrderRepositories
    {
        public OrderRepositories(HmesContext context) : base(context)
        {
        }

        public async Task<(List<Order> orders, int TotalItems)> GetAllOrdersAsync(string? keyword, decimal? minPrice, decimal? maxPrice, DateTime? startDate, DateTime? endDate,
            string? status, int pageIndex, int pageSize)
        {
            var query = Context.Orders
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(o => o.User.Name.Contains(keyword));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(o => o.TotalPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(o => o.TotalPrice <= maxPrice.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            var totalItems = await query.CountAsync();
            var orders = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (orders, totalItems);
        }

        public async Task<(List<Order> orders, int TotalItems)> GetSelfOrdersAsync(Guid userId, string? keyword, decimal? minPrice, decimal? maxPrice, DateTime? startDate, DateTime? endDate,
            string? status, int pageIndex, int pageSize)
        {
            var query = Context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(o => o.User.Name.Contains(keyword));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(o => o.TotalPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(o => o.TotalPrice <= maxPrice.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            var totalItems = await query.CountAsync();
            var orders = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (orders, totalItems);
        }

        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            var order = await Context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Device)
                .Include(o => o.UserAddress)
                .Include(o => o.Transactions)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            return order;

        }
    }
}
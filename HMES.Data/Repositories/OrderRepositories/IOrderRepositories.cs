using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.OrderRepositories
{
    public interface IOrderRepositories : IGenericRepositories<Order>
    {
        Task<(List<Order> orders, int TotalItems)> GetAllOrdersAsync(string? keyword, decimal? minPrice,
            decimal? maxPrice, DateTime? startDate, DateTime? endDate, string? status, int pageIndex, int pageSize);

        Task<(List<Order> orders, int TotalItems)> GetSelfOrdersAsync(Guid userId, string? keyword, decimal? minPrice,
        decimal? maxPrice, DateTime? startDate, DateTime? endDate, string? status, int pageIndex, int pageSize);

        Task<Order?> GetOrderByIdAsync(Guid orderId);

    }
}
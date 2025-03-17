using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.ProductRepositories;

public interface IProductRepositories:IGenericRepositories<Product>
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<(List<Product> Products, int TotalItems)> GetListWithPagination(int pageIndex, int pageSize,ProductStatusEnums? status);

    Task<(List<Product> Products, int TotalItems)> GetProductsWithPagination(
        string? keyword,
        Guid? categoryId,
        int? minAmount, int? maxAmount,
        decimal? minPrice, decimal? maxPrice,
        ProductStatusEnums? status,
        DateTime? createdAfter, DateTime? createdBefore,
        int pageIndex, int pageSize);
}

using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.ProductRepositories;

public class ProductRepositories : GenericRepositories<Product>, IProductRepositories
{
    public ProductRepositories(HmesContext context) : base(context)
    {
    }
    
    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await Context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(List<Product> Products, int TotalItems)> GetListWithPagination(int pageIndex, int pageSize,ProductStatusEnums status)
    {
        var query = Context.Products.Include(p => p.Category).Where(p => p.Status.Equals(status.ToString())).OrderByDescending(p => p.CreatedAt);
        int totalItems = await query.CountAsync();
        var products = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (products, totalItems);
    }
    
    public async Task<(List<Product> Products, int TotalItems)> GetProductsWithPagination(
        string? keyword, 
        Guid? categoryId, 
        int? minAmount, int? maxAmount,
        decimal? minPrice, decimal? maxPrice,
        ProductStatusEnums? status, 
        DateTime? createdAfter, DateTime? createdBefore,
        int pageIndex, int pageSize)
    {
        var query = Context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p => p.Name.Contains(keyword));
        }

        if (categoryId.HasValue && categoryId != Guid.Empty)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (minAmount.HasValue)
        {
            query = query.Where(p => p.Amount >= minAmount.Value);
        }

        if (maxAmount.HasValue)
        {
            query = query.Where(p => p.Amount <= maxAmount.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.ToString());
        }

        if (createdAfter.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= createdAfter.Value);
        }

        if (createdBefore.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= createdBefore.Value);
        }

        int totalItems = await query.CountAsync();
        var products = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalItems);
    }

    
    
    
}
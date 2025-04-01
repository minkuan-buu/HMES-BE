using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.TargetValueRepositories;

public class TargetValueRepositories : GenericRepositories<TargetValue>, ITargetValueRepositories
{
    
    
    
    public TargetValueRepositories(HmesContext context) : base(context)
    {
    }


    public async Task<TargetValue?> GetTargetValueByPlantId(Guid plantId)
    {
        var targetValue = await Context.TargetValues
            .Include(t => t.TargetOfPlants)
            .FirstOrDefaultAsync(t => t.TargetOfPlants.Any(tp => tp.PlantId == plantId));
        return targetValue;
    }

    public async Task<TargetValue?> GetTargetValueByTypeAndMinAndMax(string type, decimal minValue, decimal maxValue)
    {
        var targetValue = await Context.TargetValues
            .FirstOrDefaultAsync(t => t.Type.Equals(type) && t.MinValue == minValue && t.MaxValue == maxValue);
        return targetValue;
    }

    public async Task<TargetValue?> GetTargetValueById(Guid id)
    {
        var targetValue = await Context.TargetValues
            .Include(t => t.TargetOfPlants)
            .ThenInclude(tp => tp.Plant)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
        return targetValue;
    }

    public async Task<(List<TargetValue> values, int TotalItems)> GetAllValuesAsync(string? type, decimal? minValue, decimal? maxValue, int pageIndex, int pageSize)
    {
        var query = Context.TargetValues.AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(t => t.Type.Equals(type));
        }

        if (minValue != null)
        {
            query = query.Where(t => t.MinValue >= minValue);
        }

        if (maxValue != null)
        {
            query = query.Where(t => t.MaxValue <= maxValue);
        }
        
        int totalItems = await query.CountAsync();
        var values = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (values, totalItems);
    }
}
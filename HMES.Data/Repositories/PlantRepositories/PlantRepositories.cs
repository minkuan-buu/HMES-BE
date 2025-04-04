using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.PlantRepositories;

public class PlantRepositories(HmesContext context) : GenericRepositories<Plant>(context), IPlantRepositories
{
    public async Task<(List<Plant> plants, int TotalItems)> GetAllPlantsAsync(string? keyword, string? status, int pageIndex, int pageSize)
    {
        var query = Context.Plants.AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(t => t.Name.Contains(keyword));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status == status);
        }
        
        int totalItems = await query.CountAsync();
        var plants = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (plants, totalItems);
    }

    public async Task<Plant?> GetByIdAsync(Guid id)
    {
        var plant = await Context.Plants
            .Include(p => p.TargetOfPlants)
            .ThenInclude(tp => tp.TargetValue)
            .FirstOrDefaultAsync(p => p.Id == id);
        return plant;
    }

    public async Task<bool> PlantHasTargetValueType(Guid plantId, string targetType)
    {
        return await Context.Plants
            .Where(p => p.Id == plantId)
            .SelectMany(p => p.TargetOfPlants)
            .Select(top => top.TargetValue)
            .AnyAsync(tv => tv.Type == targetType);
    }

    public async Task<List<Plant>> GetPlantsWithoutTargetValueOfType(string type)
    {
        return await Context.Plants
            .Where(p => p.TargetOfPlants.All(top => top.TargetValue.Type != type))
            .ToListAsync();
    }
}
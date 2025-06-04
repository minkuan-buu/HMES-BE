using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.PlantRepositories;

public class PlantRepositories(HmesContext context) : GenericRepositories<Plant>(context), IPlantRepositories
{
    public async Task<(List<Plant> plants, int TotalItems)> GetAllPlantsAsync(string? keyword, string? status, int pageIndex, int pageSize)
    {
        var query = Context.Plants
            .OrderBy(p => p.Name)
            .AsQueryable();

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
            .Include(p => p.PlantOfPhases)
            .ThenInclude(pp => pp.Phase)
            .Include(p => p.PlantOfPhases)
            .ThenInclude(pp => pp.TargetOfPhases)
            .ThenInclude(tp => tp.TargetValue)
            .FirstOrDefaultAsync(p => p.Id == id);
        return plant;
    }

    public async Task<Plant?> GetByIdNotIncludeUserAsync(Guid id)
    {
        var plant = await Context.Plants
            .Include(p => p.PlantOfPhases.Where(pop => 
                Context.GrowthPhases.Any(gp => gp.Id == pop.PhaseId && gp.UserId == null)))
            .ThenInclude(pp => pp.Phase)
            .Include(p => p.PlantOfPhases.Where(pop => 
                Context.GrowthPhases.Any(gp => gp.Id == pop.PhaseId && gp.UserId == null)))
            .ThenInclude(pp => pp.TargetOfPhases)
            .ThenInclude(tp => tp.TargetValue)
            .FirstOrDefaultAsync(p => p.Id == id);
        return plant;
    }

    public async Task<bool> PlantHasTargetValueType(Guid plantId, string targetType, Guid phaseId)
    {
        return await Context.Plants
            .Where(p => p.Id == plantId)
            .SelectMany(p => p.PlantOfPhases.Where(pop => 
                pop.PhaseId == phaseId && 
                Context.GrowthPhases.Any(gp => gp.Id == pop.PhaseId && gp.UserId == null)))
            .SelectMany(pop => pop.TargetOfPhases)
            .Select(top => top.TargetValue)
            .AnyAsync(tv => tv.Type == targetType);
    }

    public async Task<List<Plant>> GetPlantsWithoutTargetValueOfType(string type)
    {
        var plantsWithQualifyingPhases = await Context.Plants
            .Where(p => p.PlantOfPhases.Any(pop => 
                Context.GrowthPhases.Any(gp => gp.Id == pop.PhaseId && gp.UserId == null) &&
                // Phase doesn't have the specified target type
                !pop.TargetOfPhases.Any(top => top.TargetValue.Type == type)
            ))
            .Select(p => p.Id)
            .ToListAsync();

        // Then get those plants with filtered phases
        var plants = await Context.Plants
            .Where(p => plantsWithQualifyingPhases.Contains(p.Id))
            .Include(p => p.PlantOfPhases.Where(pop => 
                Context.GrowthPhases.Any(gp => gp.Id == pop.PhaseId && gp.UserId == null) &&
                !pop.TargetOfPhases.Any(top => top.TargetValue.Type == type)
            ))
            .ThenInclude(pop => pop.Phase)
            .ToListAsync();

        return plants;
    }

    public async Task<List<Plant>> GetAllPlants()
    {
        return await Context.Plants
            .Include(p => p.PlantOfPhases)
            .ThenInclude(pp => pp.Phase)
            .Include(p => p.PlantOfPhases)
            .ThenInclude(pp => pp.TargetOfPhases)
            .ThenInclude(tp => tp.TargetValue)
            .ToListAsync();
    }
}
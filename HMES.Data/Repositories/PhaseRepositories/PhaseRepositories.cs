using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using HMES.Data.Repositories.PlantRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.PhaseRepositories;

public class PhaseRepositories(HmesContext context) : GenericRepositories<GrowthPhase>(context), IPhaseRepositories
{
    public async Task<List<GrowthPhase>> GetGrowthPhasesNoUser()
    {
        return await Context.GrowthPhases.Where(g => g.UserId == null).ToListAsync();
    }

    public async Task<(List<GrowthPhase> phases, int TotalItems)> GetAllPhasesAsync()
    {
        var query = Context.GrowthPhases
            .AsQueryable();

        var totalItems = await query.CountAsync();
        var phases = await query.ToListAsync();

        return (phases, totalItems);
    }

    public async Task<(List<GrowthPhase> phases, int TotalItems)> GetAllPhasesOfPlantAsync(Guid plantId)
    {
        var query = Context.GrowthPhases
            .Include(g => g.PlantOfPhases)
            .ThenInclude(p => p.Plant)
            .Where(g => g.PlantOfPhases.Any(p => p.PlantId == plantId))
            .AsQueryable();

        var totalItems = await query.CountAsync();
        var phases = await query.ToListAsync();
        return (phases, totalItems);
        
    }

    public async Task<GrowthPhase?> GetGrowthPhaseByUserId(Guid id)
    {
        return await Context.GrowthPhases
            .FirstOrDefaultAsync(g => g.UserId == id);
    }

    public async Task<GrowthPhase?> GetGrowthPhaseById(Guid id)
    {
        return await Context.GrowthPhases
            .Include(g => g.PlantOfPhases)
            .ThenInclude(p => p.Plant)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GrowthPhase?> GetGrowthPhaseByName(string name)
    {
        return  await Context.GrowthPhases
            .Include(g => g.PlantOfPhases)
            .ThenInclude(p => p.Plant)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Name == name.Trim()); 
    }
}
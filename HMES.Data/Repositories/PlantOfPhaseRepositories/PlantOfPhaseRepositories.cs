using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.PlantOfPhaseRepositories;

public class PlantOfPhaseRepositories(HmesContext context)
    : GenericRepositories<PlantOfPhase>(context), IPlantOfPhaseRepositories
{
    public async Task<PlantOfPhase?> GetPlantOfPhasesByPlantIdAndPhaseId(Guid? plantId, Guid? phaseId)
    {
        return await Context.PlantOfPhases
            .Include(x => x.TargetOfPhases)
            .ThenInclude(x => x.TargetValue)
            .Where(x => x.PlantId == plantId && x.PhaseId == phaseId).FirstOrDefaultAsync();
    }

    public async Task<(List<PlantOfPhase> plants, int TotalItems)> GetPhasesByPlantId(Guid plantId, Guid userId)
    {
        var query = Context.PlantOfPhases
            .Include(x => x.Phase)
            .Include(x => x.Plant)
            .Where(x => x.PlantId == plantId && (x.Phase.UserId == userId || x.Phase.UserId == null))
            .OrderBy(x => x.Phase.Name)
            .AsQueryable();

        var totalItems = await query.CountAsync();
        var plants = await query.ToListAsync();

        return (plants, totalItems);
    }

    public async Task<PlantOfPhase?> GetPlantOfPhasesByPlantIdAndPhaseNumber(Guid plantId, int phaseNumber)
    {
        return await Context.PlantOfPhases
            .Include(x => x.Phase)
            .Where(x => x.PlantId == plantId && x.Phase.PhaseNumber == phaseNumber)
            .FirstOrDefaultAsync();
    }
}

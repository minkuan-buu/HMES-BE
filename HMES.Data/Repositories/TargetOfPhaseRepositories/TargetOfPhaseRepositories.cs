using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.TargetOfPhaseRepositories;

public class TargetOfPhaseRepositories : GenericRepositories<TargetOfPhase>, ITargetOfPhaseRepository
{
    public TargetOfPhaseRepositories(HmesContext context) : base(context)
    {
    }
    

    public async Task<TargetOfPhase?> GetTargetOfPhaseById(Guid targetOfPlantId)
    {
        var targetOfPlant = await Context.TargetOfPhases
            .FirstOrDefaultAsync(top => top.Id == targetOfPlantId);
        return targetOfPlant;
    }

    public async Task<List<TargetOfPhase>> GetTargetOfPhasesByPlantOfPhaseId(Guid plantOfPhaseId)
    {
        var targetOfPlants = await Context.TargetOfPhases
            .Include(top => top.TargetValue)
            .Include(top => top.PlantOfPhase)
            .ThenInclude(p => p.Phase)
            .Where(top => top.PlantOfPhaseId == plantOfPhaseId)
            .ToListAsync();
        return targetOfPlants;
    }

    public async Task<TargetOfPhase?> GetTargetOfPlantByPlantIdAndValueId(Guid plantId, Guid valueId)
    {
        var targetOfPlant = await Context.TargetOfPhases
            .Include(top => top.TargetValue)
            .FirstOrDefaultAsync(top => top.PlantOfPhaseId == plantId && top.TargetValueId == valueId);
        return targetOfPlant;
    }

    public async Task<TargetOfPhase?> GetTargetOfPhaseByPlantOfPhaseAndType(Guid plantOfPhaseId, string type)
    {
        return await Context.TargetOfPhases
            .Include(top => top.TargetValue)
            .FirstOrDefaultAsync(top => top.PlantOfPhaseId == plantOfPhaseId && top.TargetValue.Type == type);
    }
}
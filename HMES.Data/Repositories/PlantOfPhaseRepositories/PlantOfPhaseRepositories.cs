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
            .Where(x => x.PlantId == plantId && x.PhaseId == phaseId).FirstOrDefaultAsync();
    }
}
    
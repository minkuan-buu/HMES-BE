using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.TargetOfPlantRepositories;

public class TargetOfPlantRepositories : GenericRepositories<TargetOfPlant>, ITargetOfPlantRepository
{
    public TargetOfPlantRepositories(HmesContext context) : base(context)
    {
    }
    

    public async Task<TargetOfPlant?> GetTargetOfPlantById(Guid targetOfPlantId)
    {
        var targetOfPlant = await Context.TargetOfPlants
            .FirstOrDefaultAsync(top => top.Id == targetOfPlantId);
        return targetOfPlant;
    }

    public async Task<TargetOfPlant?> GetTargetOfPlantByPlantIdAndValueId(Guid plantId, Guid valueId)
    {
        var targetOfPlant = await Context.TargetOfPlants
            .Include(top => top.TargetValue)
            .FirstOrDefaultAsync(top => top.PlantId == plantId && top.TargetValueId == valueId);
        return targetOfPlant;
    }
}
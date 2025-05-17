using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.PlantOfPhaseRepositories;

public interface IPlantOfPhaseRepositories : IGenericRepositories<PlantOfPhase>
{
    Task<PlantOfPhase?> GetPlantOfPhasesByPlantIdAndPhaseId(Guid? plantId, Guid? phaseId);
    Task<(List<PlantOfPhase> plants, int TotalItems)> GetPhasesByPlantId(Guid plantId, Guid userId);
}
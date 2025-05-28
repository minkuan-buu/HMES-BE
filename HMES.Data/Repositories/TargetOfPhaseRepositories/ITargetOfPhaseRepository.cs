using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.TargetOfPhaseRepositories;

public interface ITargetOfPhaseRepository : IGenericRepositories<TargetOfPhase>
{
    
    
    Task<TargetOfPhase?> GetTargetOfPhaseById(Guid targetOfPhaseId);
    Task<List<TargetOfPhase>> GetTargetOfPhasesByPlantOfPhaseId(Guid plantOfPhaseId);

    Task<TargetOfPhase?> GetTargetOfPlantByPlantIdAndValueId(Guid plantId, Guid valueId);
    Task<TargetOfPhase?> GetTargetOfPhaseByPlantOfPhaseAndType(Guid plantOfPhaseId, string type);
}
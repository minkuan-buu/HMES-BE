using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.TargetOfPhaseRepositories;

public interface ITargetOfPhaseRepository : IGenericRepositories<TargetOfPhase>
{
    
    
    Task<TargetOfPhase?> GetTargetOfPhaseById(Guid targetOfPhaseId);
    
    Task<TargetOfPhase?> GetTargetOfPlantByPlantIdAndValueId(Guid plantId, Guid valueId);
     
}
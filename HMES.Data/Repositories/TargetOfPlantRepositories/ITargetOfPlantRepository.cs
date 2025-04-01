using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.TargetOfPlantRepositories;

public interface ITargetOfPlantRepository : IGenericRepositories<TargetOfPlant>
{
    
    
    Task<TargetOfPlant?> GetTargetOfPlantById(Guid targetOfPlantId);
    
    Task<TargetOfPlant?> GetTargetOfPlantByPlantIdAndValueId(Guid plantId, Guid valueId);
     
}
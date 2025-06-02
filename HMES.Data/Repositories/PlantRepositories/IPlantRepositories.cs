using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.PlantRepositories;

public interface IPlantRepositories : IGenericRepositories<Plant>
{
    Task<(List<Plant> plants, int TotalItems)> GetAllPlantsAsync(string? keyword,
        string? status, int pageIndex, int pageSize);
    
    Task<Plant?> GetByIdAsync(Guid id);
    Task<Plant?> GetByIdNotIncludeUserAsync(Guid id);

    Task<bool> PlantHasTargetValueType(Guid plantId, string targetType, Guid targetId);
    Task<List<Plant>> GetPlantsWithoutTargetValueOfType(string type);
}
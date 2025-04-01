using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.PlantRepositories;

public interface IPlantRepositories : IGenericRepositories<Plant>
{
    public Task<(List<Plant> plants, int TotalItems)> GetAllPlantsAsync(string? keyword,
        string? status, int pageIndex, int pageSize);
    
    public Task<Plant?> GetByIdAsync(Guid id);

    Task<bool> PlantHasTargetValueType(Guid plantId, string targetType);
}
using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.TargetValueRepositories;

public interface ITargetValueRepositories : IGenericRepositories<TargetValue>
{
    Task<TargetValue?> GetTargetValueByPlantId(Guid plantId);
    
    Task<TargetValue?> GetTargetValueByTypeAndMinAndMax(string type, decimal minValue, decimal maxValue);
    
    Task<TargetValue?> GetTargetValueById(Guid id);
    
    public Task<(List<TargetValue> values, int TotalItems)> GetAllValuesAsync(
        string? type, 
        decimal? minValue, 
        decimal? maxValue, 
        int pageIndex, 
        int pageSize);
    
    
    
}
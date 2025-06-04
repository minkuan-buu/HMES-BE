using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.PhaseRepositories;

public interface IPhaseRepositories : IGenericRepositories<GrowthPhase>
{
    
    Task<List<GrowthPhase>> GetGrowthPhasesNoUser();
    Task<(List<GrowthPhase> phases, int TotalItems)> GetAllPhasesAsync();
    Task<(List<GrowthPhase> phases, int TotalItems)> GetAllPhasesIncludeUserAsync();
    Task<(List<GrowthPhase> phases, int TotalItems)> GetAllPhasesOfPlantAsync(Guid plantId);
    Task<GrowthPhase?> GetGrowthPhaseByIdWithTargetValue(Guid id);
    Task<GrowthPhase?> GetGrowthPhaseByUserId(Guid id);
    Task<GrowthPhase?> GetGrowthPhaseById(Guid id);
    Task<GrowthPhase?> GetGrowthPhaseByName(string name);
    Task<int> CountGrowthPhase();


    Task<int> CountDefaultGrowthPhase();
}
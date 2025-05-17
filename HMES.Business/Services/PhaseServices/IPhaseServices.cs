using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;

namespace HMES.Business.Services.PhaseServices;

public interface IPhaseServices
{
    Task<ResultModel<ListDataResultModel<PhaseResModel>>> GetAllPhasesAsync();
    Task<ResultModel<ListDataResultModel<PhaseResModel>>> GetAllPhasesIncludeUserAsync(Guid plantId,string token);
    Task<ResultModel<ListDataResultModel<PhaseResModel>>> GetAllPhasesOfPlantAsync(Guid plantId);
    Task<ResultModel<DataResultModel<PhaseResModel>>> CreateNewPhaseAsync(AddNewPhaseDto newPhase, string? token);
    Task<ResultModel<DataResultModel<PhaseResModel>>> GetPhaseByIdAsync(Guid id);
    
    Task<ResultModel<DataResultModel<PhaseResModel>>> UpdatePhaseAsync(Guid id, AddNewPhaseDto updatePhase);
    Task<ResultModel<DataResultModel<PlantAndPhaseForTargetListDto>>> SetPhaseForPlant(Guid plantId, Guid phaseId);
}
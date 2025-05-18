using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;

namespace HMES.Business.Services.PlantServices;

public interface IPlantServices
{
    Task<ResultModel<ListDataResultModel<PlantResModel>>> GetAllPlantsAsync(string? keyword, string? status, int pageIndex, int pageSize);
    
    Task<ResultModel<DataResultModel<PlantResModelWithTarget>>> GetByIdAsync(Guid id);
    
    Task<ResultModel<DataResultModel<PlantResModel>>> CreatePlantAsync(PlantReqModel plantReqModel);
    
    Task<ResultModel<DataResultModel<PlantResModel>>> UpdatePlantAsync(Guid id, PlantReqModel plantReqModel);
    
    Task<ResultModel<MessageResultModel>> DeletePlantAsync(Guid id);
    
    Task<ResultModel<MessageResultModel>> SetValueForPlant(Guid plantId, Guid targetId, Guid phaseId);
    Task<ResultModel<MessageResultModel>> SetValueForCustomPhase(Guid plantId, Guid targetId, Guid phaseId);
    
    Task<ResultModel<MessageResultModel>> RemoveValueForPlant(Guid plantId, Guid targetId, Guid phaseId);
    
    Task<ResultModel<MessageResultModel>> UpdateValueForPlant(Guid plantId, Guid targetId, Guid newTargetId, Guid phaseId);
    
    Task<ResultModel<List<PlantResModelWithTarget>>> GetPlantNotSetValueOfType(string type);


    
    
    
    
    
}
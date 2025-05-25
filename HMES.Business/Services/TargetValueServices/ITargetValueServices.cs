using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;

namespace HMES.Business.Services.TargetValueServices;

public interface ITargetValueServices
{
    
    Task<ResultModel<ListDataResultModel<TargetResModel>>> GetAllTargetValuesAsync(
        string? type, 
        decimal? minValue, decimal? maxValue, 
        int pageIndex, int pageSize);
    
    Task<ResultModel<DataResultModel<TargetResModelWithPlants>>> GetByIdAsync(Guid id);
    
    Task<ResultModel<DataResultModel<TargetResModel>>> CreateTargetValueAsync(TargetReqModel targetReqModel);
    
    Task<ResultModel<MessageResultModel>> UpdateTargetValueAsync(Guid id, TargetReqModel targetReqModel);
    
    Task<ResultModel<MessageResultModel>> DeleteTargetValueAsync(Guid id);

    Task<ResultModel<MessageResultModel>> SetValueForDevice(SetValueReqModel model);
    Task<ResultModel<MessageResultModel>> UpdateValueForDevice(SetValueReqModel model);
    Task<ResultModel<DataResultModel<TargetInPhaseDto>>> GetValueByPlantAndPhase(Guid plantId, Guid phaseId);
}

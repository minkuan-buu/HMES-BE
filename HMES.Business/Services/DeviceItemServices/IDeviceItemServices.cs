using HMES.Data.DTO.ResponseModel;

namespace HMES.Business.Services.DeviceItemServices
{
    public interface IDeviceItemServices
    {
        Task<ResultModel<DataResultModel<DeviceItemDetailResModel>>> GetDeviceItemDetailById(Guid deviceItemId, string Token);

        Task<ResultModel<MessageResultModel>> UpdateRefreshCycleHours(int refreshCycleHours, Guid deviceItemId, string Token);

        Task<ResultModel<MessageResultModel>> SetPlantForDevice(Guid deviceItemId, Guid plantId, string token);
    }
}
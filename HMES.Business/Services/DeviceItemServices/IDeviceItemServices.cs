using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;

namespace HMES.Business.Services.DeviceItemServices
{
    public interface IDeviceItemServices
    {
        Task<ResultModel<DataResultModel<DeviceItemDetailResModel>>> GetDeviceItemDetailById(Guid deviceItemId, string Token);
        Task<ResultModel<DataResultModel<DeviceItemDetailResModel>>> GetDeviceItemDetail(Guid deviceItemId);

        Task<ResultModel<MessageResultModel>> UpdateRefreshCycleHours(int refreshCycleHours, Guid deviceItemId, string Token);

        Task<ResultModel<MessageResultModel>> SetPlantForDevice(Guid deviceItemId, Guid plantId, string token);

        Task<ResultModel<IoTToken>> ActiveDevice(string token, Guid DeviceId);
        Task<DeviceItem> GetDeviceItemById(Guid deviceItemId);
    }
}
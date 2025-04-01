using HMES.Data.DTO.ResponseModel;

namespace HMES.Business.Services.DeviceItemServices
{
    public interface IDeviceItemServices
    {
        Task<ResultModel<DataResultModel<DeviceItemDetailResModel>>> GetDeviceItemDetailById(Guid deviceItemId, string Token);
    }
}
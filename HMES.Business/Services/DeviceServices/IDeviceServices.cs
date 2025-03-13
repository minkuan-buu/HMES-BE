using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMES.Business.Services.DeviceServices
{
    public interface IDeviceServices
    {
        Task<ResultModel<MessageResultModel>> CreateDevices(DeviceCreateReqModel DeviceReqModel, string token);

        // Task<ResultModel<DataResultModel<DeviceDetailResModel>>> GetDeviceDetailById(Guid DeviceId, string token);

        // Task<ResultModel<MessageResultModel>> DeleteDeviceById(Guid DeviceId, string token);
        Task<ResultModel<ListDataResultModel<ListDeviceDetailResModel>>> GetListDevice();
    }
}

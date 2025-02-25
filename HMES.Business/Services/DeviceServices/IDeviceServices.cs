using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using MeowWoofSocial.Data.DTO.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMES.Business.Services.DeviceServices
{
    public interface IDeviceServices
    {
        Task<ResultModel<DataResultModel<List<DeviceCreateResModel>>>> CreateDevices(List<DeviceCreateReqModel> DeviceReqModels, string token);

        Task<ResultModel<DataResultModel<DeviceDetailResModel>>> GetDeviceDetailById(Guid DeviceId, string token);
    }
}

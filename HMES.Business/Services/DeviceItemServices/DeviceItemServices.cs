using System.Net;
using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Repositories.DeviceItemsRepositories;

namespace HMES.Business.Services.DeviceItemServices
{
    public class DeviceItemServices : IDeviceItemServices
    {
        private readonly IDeviceItemsRepositories _deviceItemsRepositories;
        private readonly IMapper _mapper;

        public DeviceItemServices(IDeviceItemsRepositories deviceItemsRepositories, IMapper mapper)
        {
            _mapper = mapper;
            _deviceItemsRepositories = deviceItemsRepositories;
        }

        public async Task<ResultModel<DataResultModel<DeviceItemDetailResModel>>> GetDeviceItemDetailById(Guid deviceItemId, string Token)
        {
            try
            {
                var userId = Guid.Parse(Authentication.DecodeToken(Token, "userid"));
                var deviceItem = await _deviceItemsRepositories.GetSingle(x => x.Id == deviceItemId && x.UserId == userId && x.IsActive, includeProperties: "Plant,NutritionReports.NutritionReportDetails,Device");
                if (deviceItem == null)
                {
                    throw new Exception("Device item not found");
                }
                var result = _mapper.Map<DeviceItemDetailResModel>(deviceItem);
                var dataResult = new DataResultModel<DeviceItemDetailResModel>
                {
                    Data = result
                };
                return new ResultModel<DataResultModel<DeviceItemDetailResModel>>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = dataResult
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }
    }
}
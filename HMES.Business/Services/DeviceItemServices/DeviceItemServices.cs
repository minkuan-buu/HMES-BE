using System.Net;
using System.Text.Json;
using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.DeviceItemsRepositories;

namespace HMES.Business.Services.DeviceItemServices
{
    public class DeviceItemServices : IDeviceItemServices
    {
        private readonly IDeviceItemsRepositories _deviceItemsRepositories;
        private readonly IMapper _mapper;
        private readonly IMqttService _mqttService;

        public DeviceItemServices(IDeviceItemsRepositories deviceItemsRepositories, IMapper mapper, IMqttService mqttService)
        {
            _deviceItemsRepositories = deviceItemsRepositories;
            _mapper = mapper;
            _mqttService = mqttService;
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

        public async Task<ResultModel<MessageResultModel>> UpdateRefreshCycleHours(int refreshCycleHours, Guid deviceItemId, string Token)
        {
            try
            {
                var userId = Guid.Parse(Authentication.DecodeToken(Token, "userid"));
                var deviceItem = await _deviceItemsRepositories.GetSingle(x => x.Id == deviceItemId && x.UserId == userId && x.IsActive);
                if (deviceItem == null)
                {
                    throw new Exception("Device item not found");
                }
                deviceItem.RefreshCycleHours = refreshCycleHours;
                await _deviceItemsRepositories.Update(deviceItem);
                var payload = new
                {
                    deviceId = deviceItem.DeviceId,
                    refreshCycleHours = refreshCycleHours
                };
                var payloadJson = JsonSerializer.Serialize(payload);
                await _mqttService.PublishAsync($"esp32/{deviceItem.Id}/refreshCycleHours", payloadJson);
                return new ResultModel<MessageResultModel>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new MessageResultModel()
                    {
                        Message = "Update refresh cycle hours successfully"
                    }
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public async Task<ResultModel<MessageResultModel>> SetPlantForDevice(Guid deviceItemId, Guid plantId, string token)
        {
            try
            {
                var userId = Guid.Parse(Authentication.DecodeToken(token, "userid"));
                var deviceItem = await _deviceItemsRepositories.GetDeviceItemByDeviceItemIdAndUserId(deviceItemId, userId);

                if (deviceItem is not { IsActive: true })
                {
                    throw new Exception("Device item not found or not active");
                }
                deviceItem.PlantId = plantId;
                await _deviceItemsRepositories.Update(deviceItem);
                return new ResultModel<MessageResultModel>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new MessageResultModel()
                    {
                        Message = "Plant set successfully"
                    }
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public async Task<DeviceItem> GetDeviceItemById(Guid deviceItemId)
        {
            return await _deviceItemsRepositories.GetSingle(x => x.Id == deviceItemId && x.IsActive);
        }

        public async Task<ResultModel<IoTToken>> ActiveDevice(string token, Guid DeviceId)
        {
            try
            {
                var userId = new Guid(Authentication.DecodeToken(token, "userid"));
                var getDevice = await _deviceItemsRepositories.GetSingle(x => x.Id == DeviceId);
                if (getDevice.UserId == null || !getDevice.Status.Equals(DeviceItemStatusEnum.Available.ToString()) || getDevice.IsActive == true || getDevice.IsOnline == true)
                {
                    throw new Exception("Can't Active Device!");
                }
                else if (!getDevice.UserId.Equals(userId))
                {
                    throw new Exception("Access denied");
                }
                else if (getDevice == null)
                {
                    throw new Exception("Device not found!");
                }
                getDevice.IsActive = true;
                getDevice.IsOnline = true;
                getDevice.Status = DeviceItemStatusEnum.ReadyForPlanting.ToString();
                await _deviceItemsRepositories.Update(getDevice);
                string IoTToken = Authentication.CreateIoTToken(getDevice.Id, getDevice.Serial, userId.ToString());

                return new ResultModel<IoTToken>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new IoTToken()
                    {
                        Token = IoTToken,
                    }
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public async Task<ResultModel<DataResultModel<DeviceItemDetailResModel>>> GetDeviceItemDetail(Guid deviceItemId)
        {
            try
            {
                var deviceItem = await _deviceItemsRepositories.GetSingle(x => x.Id == deviceItemId && x.IsActive, includeProperties: "Plant,NutritionReports.NutritionReportDetails,Device");
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
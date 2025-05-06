using System.Net;
using System.Text.Json;
using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.DeviceItemsRepositories;
using HMES.Data.Repositories.NutritionRDRepositories;
using HMES.Data.Repositories.NutritionReportRepositories;
using HMES.Data.Repositories.PlantRepositories;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace HMES.Business.Services.DeviceItemServices
{
    public class DeviceItemServices : IDeviceItemServices
    {
        private readonly INutritionRDRepositories _nutritionRDRepositories;
        private readonly INutritionReportRepositories _nutritionReportRepositories;
        private readonly IDeviceItemsRepositories _deviceItemsRepositories;
        private readonly IPlantRepositories _plantRepositories;
        private readonly IMapper _mapper;
        private readonly IMqttService _mqttService;

        public DeviceItemServices(IDeviceItemsRepositories deviceItemsRepositories, IMapper mapper, IMqttService mqttService, IPlantRepositories plantRepositories, INutritionRDRepositories nutritionRDRepositories, INutritionReportRepositories nutritionReportRepositories)
        {
            _nutritionRDRepositories = nutritionRDRepositories;
            _nutritionReportRepositories = nutritionReportRepositories;
            _deviceItemsRepositories = deviceItemsRepositories;
            _plantRepositories = plantRepositories;
            _mapper = mapper;
            _mqttService = mqttService;
        }

        public async Task<ResultModel<DataResultModel<DeviceItemDetailResModel>>> GetDeviceItemDetailById(Guid deviceItemId, string Token)
        {
            try
            {
                var userId = Guid.Parse(Authentication.DecodeToken(Token, "userid"));
                var deviceItem = await _deviceItemsRepositories.GetSingle(x => x.Id == deviceItemId && x.UserId == userId && x.IsActive, includeProperties: "Plant,NutritionReports.NutritionReportDetails.TargetValue,Device");
                if (deviceItem == null)
                {
                    throw new Exception("Device item not found");
                }
                var result = _mapper.Map<DeviceItemDetailResModel>(deviceItem);

                var nutritionReport = deviceItem.NutritionReports.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                if (nutritionReport != null)
                {
                    var newIoTResModel = new IoTResModel
                    {
                        Temperature = nutritionReport.NutritionReportDetails.FirstOrDefault(x => x.TargetValue.Type == "Temperature")?.RecordValue ?? 0,
                        SoluteConcentration = nutritionReport.NutritionReportDetails.FirstOrDefault(x => x.TargetValue.Type == "SoluteConcentration")?.RecordValue ?? 0,
                        Ph = nutritionReport.NutritionReportDetails.FirstOrDefault(x => x.TargetValue.Type == "Ph")?.RecordValue ?? 0,
                        WaterLevel = nutritionReport.NutritionReportDetails.FirstOrDefault(x => x.TargetValue.Type == "WaterLevel")?.RecordValue ?? 0,
                    };
                    result.IoTData = newIoTResModel;
                }

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
                    deviceId = deviceItem.Id,
                    refreshCycleHours = refreshCycleHours
                };
                await _mqttService.PublishAsync($"esp32/{deviceItem.Id.ToString().ToUpper()}/refreshCycleHours", payload);
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
                var plant = await _plantRepositories.GetSingle(x => x.Id == getDevice.PlantId && x.Status.Equals(GeneralStatusEnums.Inactive.ToString()));
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
                else if (plant != null)
                {
                    getDevice.PlantId = plant.Id;
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

        public async Task<ResultModel<MessageResultModel>> UpdateLog(UpdateLogIoT deviceItem, string token, Guid DeviceId)
        {
            try
            {
                var userId = Guid.Parse(Authentication.DecodeToken(token, "userid"));
                var getDevice = await _deviceItemsRepositories.GetSingle(x => x.Id == DeviceId && x.UserId == userId && x.IsActive, includeProperties: "Plant.TargetOfPlants.TargetValue");

                if (getDevice == null)
                {
                    throw new Exception("Device item not found");
                }
                else if (getDevice.Plant == null)
                {
                    throw new Exception("Active device before update log!");
                }

                var targetValues = getDevice.Plant.TargetOfPlants.Where(x => x.PlantId == getDevice.PlantId)
                .Select(x => new
                {
                    x.TargetValue.Id,
                    x.TargetValue.Type,
                    x.TargetValue.MinValue,
                    x.TargetValue.MaxValue
                }).ToList();

                var vietnameseMap = new Dictionary<string, string>
                {
                    { "Temperature", "Nhiệt độ" },
                    { "SoluteConcentration", "Nồng độ chất hòa tan" },
                    { "Ph", "Độ pH" },
                    { "WaterLevel", "Mực nước" }
                };

                string messageWarning = string.Empty;
                var newNutritionReportId = Guid.NewGuid();
                var nutritionReport = new NutritionReport
                {
                    Id = newNutritionReportId,
                    DeviceItemId = getDevice.Id,
                    CreatedAt = DateTime.Now,
                };
                await _nutritionReportRepositories.Insert(nutritionReport);
                List<NutritionReportDetail> nutritionReportDetails = new List<NutritionReportDetail>();

                for (int i = 0; i < targetValues.Count; i++)
                {
                    var targetValue = targetValues[i];
                    var value = deviceItem.GetType().GetProperty(targetValue.Type)?.GetValue(deviceItem, null);
                    var nutritionReportDetail = new NutritionReportDetail
                    {
                        TargetValueId = targetValue.Id,
                        RecordValue = value != null ? (decimal)value : 0,
                        NutritionId = newNutritionReportId,
                        Id = Guid.NewGuid(),
                    };
                    nutritionReportDetails.Add(nutritionReportDetail);

                    if (value != null && (decimal)value < targetValue.MinValue)
                    {
                        var fieldName = vietnameseMap.ContainsKey(targetValue.Type)
                            ? vietnameseMap[targetValue.Type]
                            : targetValue.Type;

                        messageWarning += $"{fieldName} đang thấp hơn ngưỡng khuyến nghị! ";
                    }
                    else if (value != null && (decimal)value > targetValue.MaxValue)
                    {
                        var fieldName = vietnameseMap.ContainsKey(targetValue.Type)
                            ? vietnameseMap[targetValue.Type]
                            : targetValue.Type;

                        messageWarning += $"{fieldName} đang cao hơn ngưỡng khuyến nghị! ";
                    }
                }
                await _nutritionRDRepositories.InsertRange(nutritionReportDetails);
                await _mqttService.PublishAsync($"push/notification/{getDevice.UserId.ToString().ToLower()}", new
                {
                    message = messageWarning,
                });

                return new ResultModel<MessageResultModel>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new MessageResultModel()
                    {
                        Message = "Update log successfully"
                    }
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public async Task<ResultModel<MessageResultModel>> UpdateLog(UpdateLogIoT deviceItem, Guid DeviceId)
        {
            try
            {
                var getDevice = await _deviceItemsRepositories.GetSingle(x => x.Id == DeviceId && x.IsActive, includeProperties: "Plant.TargetOfPlants.TargetValue");

                if (getDevice == null)
                {
                    throw new Exception("Device item not found");
                }
                else if (getDevice.Plant == null)
                {
                    throw new Exception("Active device before update log!");
                }

                var targetValues = getDevice.Plant.TargetOfPlants.Where(x => x.PlantId == getDevice.PlantId)
                .Select(x => new
                {
                    x.TargetValue.Id,
                    x.TargetValue.Type,
                    x.TargetValue.MinValue,
                    x.TargetValue.MaxValue
                }).ToList();

                var vietnameseMap = new Dictionary<string, string>
                {
                    { "Temperature", "Nhiệt độ" },
                    { "SoluteConcentration", "Nồng độ chất hòa tan" },
                    { "Ph", "Độ pH" },
                    { "WaterLevel", "Mực nước" }
                };

                string messageWarning = string.Empty;
                var newNutritionReportId = Guid.NewGuid();
                var nutritionReport = new NutritionReport
                {
                    Id = newNutritionReportId,
                    DeviceItemId = getDevice.Id,
                    CreatedAt = DateTime.Now,
                };
                await _nutritionReportRepositories.Insert(nutritionReport);
                List<NutritionReportDetail> nutritionReportDetails = new List<NutritionReportDetail>();

                for (int i = 0; i < targetValues.Count; i++)
                {
                    var targetValue = targetValues[i];
                    var value = deviceItem.GetType().GetProperty(targetValue.Type)?.GetValue(deviceItem, null);
                    var nutritionReportDetail = new NutritionReportDetail
                    {
                        TargetValueId = targetValue.Id,
                        RecordValue = value != null ? (decimal)value : 0,
                        NutritionId = newNutritionReportId,
                        Id = Guid.NewGuid(),
                    };
                    nutritionReportDetails.Add(nutritionReportDetail);

                    if (value != null && (decimal)value < targetValue.MinValue)
                    {
                        var fieldName = vietnameseMap.ContainsKey(targetValue.Type)
                            ? vietnameseMap[targetValue.Type]
                            : targetValue.Type;

                        messageWarning += $"{fieldName} đang thấp hơn ngưỡng khuyến nghị! ";
                    }
                    else if (value != null && (decimal)value > targetValue.MaxValue)
                    {
                        var fieldName = vietnameseMap.ContainsKey(targetValue.Type)
                            ? vietnameseMap[targetValue.Type]
                            : targetValue.Type;

                        messageWarning += $"{fieldName} đang cao hơn ngưỡng khuyến nghị! ";
                    }
                }
                await _nutritionRDRepositories.InsertRange(nutritionReportDetails);
                await _mqttService.PublishAsync($"push/notification/{getDevice.UserId.ToString().ToLower()}", JsonSerializer.Serialize(new
                {
                    message = messageWarning,
                }));

                return new ResultModel<MessageResultModel>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new MessageResultModel()
                    {
                        Message = "Update log successfully"
                    }
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }
    }
}
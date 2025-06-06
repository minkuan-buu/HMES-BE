using System.Net;
using System.Text.Json;
using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.Converter;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.DeviceItemsRepositories;
using HMES.Data.Repositories.NotificationRepositories;
using HMES.Data.Repositories.NutritionRDRepositories;
using HMES.Data.Repositories.NutritionReportRepositories;
using HMES.Data.Repositories.PhaseRepositories;
using HMES.Data.Repositories.PlantOfPhaseRepositories;
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
        private readonly INotificationRepositories _notificationRepositories;
        private readonly IPlantOfPhaseRepositories _plantOfPhaseRepositories;
        private readonly IPhaseRepositories _phaseRepositories;
        private readonly IMapper _mapper;
        private readonly IMqttService _mqttService;

        public DeviceItemServices(IPlantOfPhaseRepositories plantOfPhaseRepositories, IDeviceItemsRepositories deviceItemsRepositories, IMapper mapper, IMqttService mqttService, IPlantRepositories plantRepositories, INutritionRDRepositories nutritionRDRepositories, INutritionReportRepositories nutritionReportRepositories, INotificationRepositories notificationRepositories, IPhaseRepositories phaseRepositories)
        {
            _plantOfPhaseRepositories = plantOfPhaseRepositories;
            _notificationRepositories = notificationRepositories;
            _nutritionRDRepositories = nutritionRDRepositories;
            _nutritionReportRepositories = nutritionReportRepositories;
            _deviceItemsRepositories = deviceItemsRepositories;
            _plantRepositories = plantRepositories;
            _mapper = mapper;
            _mqttService = mqttService;
            _phaseRepositories = phaseRepositories;
        }

        public async Task<ResultModel<DataResultModel<DeviceItemDetailResModel>>> GetDeviceItemDetailById(Guid deviceItemId, string Token)
        {
            try
            {
                var userId = Guid.Parse(Authentication.DecodeToken(Token, "userid"));
                var deviceItem = await _deviceItemsRepositories.GetSingle(x => x.Id == deviceItemId && x.UserId == userId && x.IsActive, includeProperties: "Plant,NutritionReports.NutritionReportDetails.TargetValue,Device");
                if (deviceItem.PlantId == null)
                {
                    throw new Exception("Device item does not have a PlantId assigned.");
                }
                var phase = await _phaseRepositories.GetList(x => x.PlantOfPhases.Any(p => p.PlantId == deviceItem.PlantId) && (x.UserId == null || x.UserId == userId), includeProperties: "PlantOfPhases.Plant");
                if (deviceItem == null)
                {
                    throw new Exception("Device item not found");
                }
                var result = _mapper.Map<DeviceItemDetailResModel>(deviceItem);
                result.Phase = phase.ToList().Select(x => new PhaseDeviceDetailModel
                {
                    Id = x.Id,
                    PhaseName = TextConvert.ConvertFromUnicodeEscape(x.Name),
                    IsSelected = x.Id == deviceItem.PhaseId,
                    IsDefault = x.UserId == null,
                }).ToList();
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

                // var phase = await _plantOfPhaseRepositories.GetPlantOfPhasesByPlantIdAndPhaseNumber(plantId, 1);
                // if (phase == null)
                // {
                //     throw new Exception("Plant not has default phase");
                // }
                deviceItem.PlantId = plantId;
                deviceItem.PhaseId = null;
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

        public async Task<ResultModel<MessageResultModel>> SetPhaseForDevice(Guid deviceItemId, Guid phaseId, string token)
        {
            try
            {
                var userId = Guid.Parse(Authentication.DecodeToken(token, "userid"));
                var deviceItem = await _deviceItemsRepositories.GetDeviceItemByDeviceItemIdAndUserId(deviceItemId, userId);

                if (deviceItem is not { IsActive: true })
                {
                    throw new Exception("Device item not found or not active");
                }
                deviceItem.PhaseId = phaseId;
                await _deviceItemsRepositories.Update(deviceItem);
                return new ResultModel<MessageResultModel>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new MessageResultModel()
                    {
                        Message = "Phase set successfully"
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

        public async Task<ResultModel<IoTToken>> ActiveDevice(string token, DeviceActveReqModel reqModel)
        {
            try
            {
                var userId = new Guid(Authentication.DecodeToken(token, "userid"));
                var getDevice = await _deviceItemsRepositories.GetSingle(x => x.Id == reqModel.DeviceItemId);
                var plant = await _plantRepositories.GetSingle(x => x.Id == getDevice.PlantId && x.Status.Equals(GeneralStatusEnums.Inactive.ToString()));
                if ((getDevice.UserId == null || !getDevice.Status.Equals(DeviceItemStatusEnum.Available.ToString()) || getDevice.IsActive == true || getDevice.IsOnline == true) && reqModel.IsReconnect == false)
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

        public async Task<ResultModel<MessageResultModel>> DeactiveDevice(Guid DeviceId)
        {
            try
            {
                var getDevice = await _deviceItemsRepositories.GetSingle(x => x.Id == DeviceId);
                var plant = await _plantRepositories.GetSingle(x => x.Id == getDevice.PlantId && x.Status.Equals(GeneralStatusEnums.Inactive.ToString()));
                if (getDevice.UserId == null || getDevice.Status.Equals(DeviceItemStatusEnum.Available.ToString()) || getDevice.IsActive == false || getDevice.IsOnline == false)
                {
                    throw new Exception("Can't Deactive Device!");
                }
                else if (getDevice == null)
                {
                    throw new Exception("Device not found!");
                }
                else if (plant != null)
                {
                    getDevice.PlantId = plant.Id;
                }
                getDevice.IsActive = false;
                getDevice.IsOnline = false;
                getDevice.Status = DeviceItemStatusEnum.Available.ToString();
                getDevice.LastSeen = null;
                await _deviceItemsRepositories.Update(getDevice);

                return new ResultModel<MessageResultModel>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new MessageResultModel()
                    {
                        Message = "Deactive Device successfully!",
                    }
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
                var getDevice = await _deviceItemsRepositories.GetSingle(x => x.Id == DeviceId && x.UserId == userId && x.IsActive, includeProperties: "Plant.PlantOfPhases.TargetOfPhases.TargetValue");

                if (getDevice == null)
                {
                    throw new Exception("Device item not found");
                }
                else if (getDevice.Plant == null)
                {
                    throw new Exception("Active device before update log!");
                }

                var plantPhase = await _plantOfPhaseRepositories.GetPlantOfPhasesByPlantIdAndPhaseId(getDevice.PlantId, getDevice.PhaseId);

                if (plantPhase == null)
                {
                    throw new Exception("Set plant and phase before update log!");
                }

                var targetValues = plantPhase.TargetOfPhases
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

                        messageWarning += $"{fieldName} đang thấp hơn ngưỡng khuyến nghị!\n";
                    }
                    else if (value != null && (decimal)value > targetValue.MaxValue)
                    {
                        var fieldName = vietnameseMap.ContainsKey(targetValue.Type)
                            ? vietnameseMap[targetValue.Type]
                            : targetValue.Type;

                        messageWarning += $"{fieldName} đang cao hơn ngưỡng khuyến nghị!\n";
                    }
                }
                await _nutritionRDRepositories.InsertRange(nutritionReportDetails);
                Notification notification = new Notification()
                {
                    Id = Guid.NewGuid(),
                    UserId = getDevice.UserId ?? throw new Exception("UserId is null"),
                    CreatedAt = DateTime.Now,
                    Message = TextConvert.ConvertToUnicodeEscape(messageWarning),
                    NotificationType = NotificationTypeEnums.DeviceItems.ToString(),
                    IsRead = false,
                    ReadAt = null,
                    ReferenceId = getDevice.Id,
                    SenderId = null,
                    Title = TextConvert.ConvertToUnicodeEscape($"Cảnh báo từ thiết bị {TextConvert.ConvertToUnicodeEscape(getDevice.Name)}"),
                };
                await _notificationRepositories.Insert(notification);
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
                var getDevice = await _deviceItemsRepositories.GetSingle(x => x.Id == DeviceId && x.IsActive, includeProperties: "Plant.PlantOfPhases.TargetOfPhases.TargetValue");

                if (getDevice == null)
                {
                    throw new Exception("Device item not found");
                }
                else if (getDevice.Plant == null)
                {
                    throw new Exception("Active device before update log!");
                }

                var plantPhase = await _plantOfPhaseRepositories.GetPlantOfPhasesByPlantIdAndPhaseId(getDevice.PlantId, getDevice.PhaseId);

                if (plantPhase == null)
                {
                    throw new Exception("Set plant and phase before update log!");
                }

                var targetValues = plantPhase.TargetOfPhases
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

                        messageWarning += $"{fieldName} đang thấp hơn ngưỡng khuyến nghị!\n";
                    }
                    else if (value != null && (decimal)value > targetValue.MaxValue)
                    {
                        var fieldName = vietnameseMap.ContainsKey(targetValue.Type)
                            ? vietnameseMap[targetValue.Type]
                            : targetValue.Type;

                        messageWarning += $"{fieldName} đang cao hơn ngưỡng khuyến nghị!\n";
                    }
                }
                await _nutritionRDRepositories.InsertRange(nutritionReportDetails);
                Notification notification = new Notification()
                {
                    Id = Guid.NewGuid(),
                    UserId = getDevice.UserId ?? throw new Exception("UserId is null"),
                    CreatedAt = DateTime.Now,
                    Message = TextConvert.ConvertToUnicodeEscape(messageWarning),
                    NotificationType = NotificationTypeEnums.DeviceItems.ToString(),
                    IsRead = false,
                    ReadAt = null,
                    ReferenceId = getDevice.Id,
                    SenderId = null,
                    Title = TextConvert.ConvertToUnicodeEscape($"Cảnh báo từ thiết bị {TextConvert.ConvertToUnicodeEscape(getDevice.Name)}"),
                };
                await _notificationRepositories.Insert(notification);
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

        public async Task<ResultModel<DataResultModel<HistoryLogIoTResModel>>> GetHistoryLog(Guid deviceItemId, string token)
        {
            try
            {
                var userId = Guid.Parse(Authentication.DecodeToken(token, "userid"));
                var deviceItem = await _deviceItemsRepositories.GetSingle(x => x.Id == deviceItemId && x.UserId == userId && x.IsActive, includeProperties: "Plant,NutritionReports.NutritionReportDetails.TargetValue,Device");
                if (deviceItem == null)
                {
                    throw new Exception("Device item not found");
                }
                var result = _mapper.Map<HistoryLogIoTResModel>(deviceItem);
                var nutritionReports = deviceItem.NutritionReports.OrderByDescending(x => x.CreatedAt).ToList();
                var nutritionReportDetails = new List<IoTHistoryResModel>();
                foreach (var nutritionReport in nutritionReports)
                {
                    var nutritionReportDetail = new IoTHistoryResModel
                    {
                        NutrionId = nutritionReport.Id,
                        SoluteConcentration = nutritionReport.NutritionReportDetails.FirstOrDefault(x => x.TargetValue.Type == "SoluteConcentration")?.RecordValue ?? 0,
                        Temperature = nutritionReport.NutritionReportDetails.FirstOrDefault(x => x.TargetValue.Type == "Temperature")?.RecordValue ?? 0,
                        Ph = nutritionReport.NutritionReportDetails.FirstOrDefault(x => x.TargetValue.Type == "Ph")?.RecordValue ?? 0,
                        WaterLevel = nutritionReport.NutritionReportDetails.FirstOrDefault(x => x.TargetValue.Type == "WaterLevel")?.RecordValue ?? 0,
                        CreatedAt = nutritionReport.CreatedAt,
                    };
                    nutritionReportDetails.Add(nutritionReportDetail);
                }
                result.IoTData = nutritionReportDetails;
                var dataResult = new DataResultModel<HistoryLogIoTResModel>
                {
                    Data = result
                };
                return new ResultModel<DataResultModel<HistoryLogIoTResModel>>()
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

        public async Task<ResultModel<MessageResultModel>> UpdateNameDeviceItem(UpdateNameDeviceItem request, Guid deviceItemId, string token)
        {
            try
            {
                var userId = Guid.Parse(Authentication.DecodeToken(token, "userid"));
                var deviceItem = await _deviceItemsRepositories.GetSingle(x => x.Id == deviceItemId && x.UserId == userId && x.IsActive);
                if (deviceItem == null)
                {
                    throw new Exception("Device item not found");
                }
                deviceItem.Name = TextConvert.ConvertToUnicodeEscape(request.DeviceItemName);
                await _deviceItemsRepositories.Update(deviceItem);
                return new ResultModel<MessageResultModel>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new MessageResultModel()
                    {
                        Message = "Update name device item successfully"
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
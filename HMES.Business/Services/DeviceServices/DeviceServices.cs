using AutoMapper;
using HMES.Business.Services.CloudServices;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.Converter;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.DeviceRepositories;
using HMES.Data.Repositories.UserRepositories;
using HMES.Data.Repositories.UserTokenRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HMES.Business.Services.DeviceServices
{
    public class DeviceServices : IDeviceServices
    {
        private readonly IUserRepositories _userRepositories;
        private readonly IMapper _mapper;
        private readonly IDeviceRepositories _deviceRepositories;
        private readonly ICloudServices _cloudServices;

        public DeviceServices(IUserRepositories userRepositories, IMapper mapper, IDeviceRepositories deviceRepositories, ICloudServices cloudServices)
        {
            _userRepositories = userRepositories;
            _mapper = mapper;
            _deviceRepositories = deviceRepositories;
            _cloudServices = cloudServices;
        }

        public async Task<ResultModel<MessageResultModel>> CreateDevices(DeviceCreateReqModel DeviceReqModel, string token)
        {
            var deviceEntities = new List<Device>();
            try
            {
                for (int i = 0; i < DeviceReqModel.Quantity; i++)
                {
                    var newDeviceId = Guid.NewGuid();
                    var DeviceEntity = _mapper.Map<Device>(DeviceReqModel);
                    DeviceEntity.Id = newDeviceId;
                    DeviceEntity.Name = TextConvert.ConvertToUnicodeEscape(DeviceReqModel.Name);
                    string filePath = $"device/{DeviceEntity.Id}/attachments";
                    if (DeviceReqModel.Attachment != null)
                    {
                        var attachments = await _cloudServices.UploadSingleFile(DeviceReqModel.Attachment, filePath);
                        DeviceEntity.Attachment = attachments;
                    }
                    DeviceEntity.Status = DeviceStatusEnum.Deactive.ToString();
                    DeviceEntity.IsActive = false;
                    DeviceEntity.IsOnline = false;
                    DeviceEntity.Serial = Authentication.GenerateRandomSerial(24);
                    DeviceEntity.Price = DeviceReqModel.Price;

                    deviceEntities.Add(DeviceEntity);
                }


                await _deviceRepositories.InsertRange(deviceEntities);

                return new ResultModel<MessageResultModel>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new MessageResultModel()
                    {
                        Message = "Device is created!"
                    }
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }

        }

        public async Task<ResultModel<DataResultModel<DeviceDetailResModel>>> GetDeviceDetailById(Guid DeviceId, string token)
        {
            var result = new DataResultModel<DeviceDetailResModel>();

            try
            {
                Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
                var deviceDetail = await _deviceRepositories.GetSingle(x => x.Id == DeviceId,
                includeProperties: "NutritionReports");
                if (deviceDetail == null)
                {
                    throw new Exception("Device not found!");
                } else if (!deviceDetail.UserId.Equals(userId))
                {
                    throw new Exception("Access denied");
                }

                var deviceResModel = _mapper.Map<DeviceDetailResModel>(deviceDetail);
                result.Data = deviceResModel;

                return new ResultModel<DataResultModel<DeviceDetailResModel>>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public async Task<ResultModel<MessageResultModel>> DeleteDeviceById(Guid DeviceId, string token)
        {
            try
            {
                Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
                var getDevice = await _deviceRepositories.GetSingle(x => x.Id == DeviceId);
                if (getDevice.UserId == null || getDevice.Status.Equals(DeviceStatusEnum.Active.ToString()) || getDevice.IsActive == true || getDevice.IsOnline == true) 
                {
                    throw new Exception("Cann't Delete Device!");
                }
                else if (!getDevice.UserId.Equals(userId))
                {
                    throw new Exception("Access denied");
                }
                else if (getDevice == null)
                {
                    throw new Exception("Device not found!");
                }

                await _deviceRepositories.Delete(getDevice);

                return new ResultModel<MessageResultModel>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new MessageResultModel()
                    {
                        Message = "Device is deleted!"
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

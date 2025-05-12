using AutoMapper;
using HMES.Business.Services.CloudServices;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.Converter;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.DeviceItemsRepositories;
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
        private readonly IDeviceItemsRepositories _deviceItemsRepositories;

        public DeviceServices(IUserRepositories userRepositories, IMapper mapper, IDeviceRepositories deviceRepositories, ICloudServices cloudServices, IDeviceItemsRepositories deviceItemsRepositories)
        {
            _userRepositories = userRepositories;
            _mapper = mapper;
            _deviceRepositories = deviceRepositories;
            _cloudServices = cloudServices;
            _deviceItemsRepositories = deviceItemsRepositories;
        }

        public async Task<ResultModel<MessageResultModel>> CreateDevices(DeviceCreateReqModel DeviceReqModel, string token)
        {
            try
            {
                var newDeviceId = Guid.NewGuid();
                var DeviceEntity = _mapper.Map<Device>(DeviceReqModel);
                DeviceEntity.Id = newDeviceId;

                string filePath = $"device/{DeviceEntity.Id}/attachments";
                if (DeviceReqModel.Attachment != null)
                {
                    var attachments = await _cloudServices.UploadSingleFile(DeviceReqModel.Attachment, filePath);
                    DeviceEntity.Attachment = attachments;
                }
                DeviceEntity.Status = DeviceStatusEnum.Active.ToString();

                await _deviceRepositories.Insert(DeviceEntity);

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

        public async Task<ResultModel<DataResultModel<DeviceDetailResModel>>> GetDeviceDetailById(Guid DeviceId)
        {
            var result = new DataResultModel<DeviceDetailResModel>();

            try
            {
                var deviceDetail = await _deviceRepositories.GetSingle(x => x.Id.Equals(DeviceId));
                if (deviceDetail == null)
                {
                    throw new Exception("Device not found!");
                }
                else if (deviceDetail.Status.Equals(DeviceStatusEnum.Deactive.ToString()))
                {
                    throw new Exception("Can't view detail of this device!");
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

        public async Task<ResultModel<ListDataResultModel<ListMyDeviceResModel>>> GetListDeviceByUserId(string token)
        {
            try
            {
                var userId = new Guid(Authentication.DecodeToken(token, "userid"));
                var devices = await _deviceItemsRepositories.GetList(x => x.UserId.Equals(userId));
                if (devices == null)
                {
                    throw new Exception("Device not found!");
                }
                var resultList = _mapper.Map<List<ListMyDeviceResModel>>(devices);
                var result = new ListDataResultModel<ListMyDeviceResModel>()
                {
                    Data = resultList
                };
                return new ResultModel<ListDataResultModel<ListMyDeviceResModel>>()
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
                var deviceDetail = await _deviceRepositories.GetSingle(x => x.Id.Equals(DeviceId));
                if (deviceDetail == null)
                {
                    throw new Exception("Device not found!");
                }
                else if (deviceDetail.Status.Equals(DeviceStatusEnum.Deactive.ToString()))
                {
                    throw new Exception("Can't delete this device!");
                }

                deviceDetail.Status = DeviceStatusEnum.Deactive.ToString();
                await _deviceRepositories.Update(deviceDetail);

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

        public async Task<ResultModel<ListDataResultModel<ListDeviceDetailResModel>>> GetListDevice()
        {
            var result = new ListDataResultModel<ListDeviceDetailResModel>();

            try
            {
                var deviceDetails = await _deviceRepositories.GetList(x => x.Status.Equals(DeviceStatusEnum.Active.ToString()));
                if (deviceDetails == null || !deviceDetails.Any())
                {
                    throw new Exception("Device not found!");
                }

                var deviceResModels = _mapper.Map<List<ListDeviceDetailResModel>>(deviceDetails);
                result.Data = deviceResModels;

                return new ResultModel<ListDataResultModel<ListDeviceDetailResModel>>()
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

        public async Task<ResultModel<ListDataResultModel<ListActiveDeviceResModel>>> GetListActiveDeviceByUserId(string token)
        {
            var result = new ListDataResultModel<ListActiveDeviceResModel>();

            try
            {
                var userId = new Guid(Authentication.DecodeToken(token, "userid"));
                var deviceDetails = await _deviceItemsRepositories.GetList(x => x.UserId.Equals(userId) && x.IsActive, includeProperties: "Plant");
                if (deviceDetails == null || !deviceDetails.Any())
                {
                    throw new Exception("Device not found!");
                }

                var deviceResModels = _mapper.Map<List<ListActiveDeviceResModel>>(deviceDetails);
                result.Data = deviceResModels;

                return new ResultModel<ListDataResultModel<ListActiveDeviceResModel>>()
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
    }
}

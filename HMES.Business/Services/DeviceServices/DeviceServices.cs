﻿using AutoMapper;
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
using MeowWoofSocial.Data.DTO.ResponseModel;
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

        public async Task<ResultModel<DataResultModel<List<DeviceCreateResModel>>>> CreateDevices(List<DeviceCreateReqModel> DeviceReqModels, string token)
        {
            var result = new DataResultModel<List<DeviceCreateResModel>>();
            var deviceEntities = new List<Device>();
            try
            {
                Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
                var user = await _userRepositories.GetSingle(x => x.Id == userId);

                //cai nay se thay doi code sau, check xem co phai admin khong, tam thoi de status cua account, doi thanh role sau
                if (user == null || user.Status.Equals(AccountStatusEnums.Inactive))
                {
                    throw new CustomException("You are banned from creating device due to violate of terms!");
                }

                foreach (var DeviceReqModel in DeviceReqModels)
                {
                    var newDeviceId = Guid.NewGuid();
                    var DeviceEntity = _mapper.Map<Device>(DeviceReqModel);
                    DeviceEntity.Id = newDeviceId;
                    DeviceEntity.UserId = userId;
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

                foreach (var deviceEntity in deviceEntities)
                {
                    await _deviceRepositories.Insert(deviceEntity);
                }
                result.Data = _mapper.Map<List<DeviceCreateResModel>>(deviceEntities);
            }
            catch (Exception ex)
            {
                return new ResultModel<DataResultModel<List<DeviceCreateResModel>>>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = result
                };
            }
            return new ResultModel<DataResultModel<List<DeviceCreateResModel>>>()
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = result
            };
        }
    }
}

using System.Net;
using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.Converter;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.UserAddressRepositories;
using HMES.Data.Repositories.UserRepositories;
using Newtonsoft.Json.Linq;

namespace HMES.Business.Services.UserAddressServices;

public class UserAddressServices : IUserAddressServices
{
    private readonly IUserAddressRepositories _userAddressRepo;
    private readonly IMapper _mapper;
    private readonly IUserRepositories _userRepo;
    public UserAddressServices(IUserAddressRepositories userAddressRepo, IMapper mapper, IUserRepositories userRepo)
    {
        _userAddressRepo = userAddressRepo;
        _mapper = mapper;
        _userRepo = userRepo;
    }

    public async Task<ResultModel<MessageResultModel>> CreateUserAddress(UserAddressCreateReqModel userAddressReq, string token)
    {
        try
        {
            Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
            var user = await _userRepo.GetSingle(x => x.Id == userId);

            if (user == null || user.Status.Equals(AccountStatusEnums.Inactive))
            {
                throw new CustomException("You are banned from creating address due to violation of terms!");
            }

            var userAddresses = await _userAddressRepo.GetList(x => x.UserId.Equals(userId));

            var (latitude, longitude) = await GetCoordinatesFromHereAsync(userAddressReq.Address);

            var newUserAddressId = Guid.NewGuid();
            var userAddressEntity = _mapper.Map<UserAddress>(userAddressReq);
            userAddressEntity.Id = newUserAddressId;
            userAddressEntity.UserId = userId;
            userAddressEntity.CreatedAt = DateTime.Now;
            userAddressEntity.Ward = TextConvert.ConvertToUnicodeEscape(userAddressReq.Ward);
            userAddressEntity.District = TextConvert.ConvertToUnicodeEscape(userAddressReq.District);
            userAddressEntity.Province = TextConvert.ConvertToUnicodeEscape(userAddressReq.Province);
            userAddressEntity.Status = userAddresses.Any() ? UserAddressEnums.Active.ToString() : UserAddressEnums.Default.ToString();

            if (latitude.HasValue && longitude.HasValue)
            {
                userAddressEntity.Latitude = (decimal)latitude.Value;
                userAddressEntity.Longitude = (decimal)longitude.Value;
            }

            await _userAddressRepo.Insert(userAddressEntity);

            return new ResultModel<MessageResultModel>()
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new MessageResultModel()
                {
                    Message = "Address is created!"
                }
            };
        }
        catch (Exception ex)
        {
            throw new CustomException($"An error occurred: {ex.Message}");
        }
    }


    private async Task<(double? Latitude, double? Longitude)> GetCoordinatesFromHereAsync(string fullAddress)
    {
        try
        {
            string apiKey = Environment.GetEnvironmentVariable("HERE_MAP_API_KEY") ?? throw new Exception("HERE Map API Key is missing");
            string url = $"https://geocode.search.hereapi.com/v1/geocode?q={Uri.EscapeDataString(fullAddress)}&apiKey={apiKey}";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return (null, null);
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseBody);

                var location = json["items"]?.FirstOrDefault()?["position"];
                if (location != null)
                {
                    double latitude = location["lat"]?.Value<double>() ?? 0;
                    double longitude = location["lng"]?.Value<double>() ?? 0;
                    return (latitude, longitude);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting coordinates: {ex.Message}");
        }

        return (null, null);
    }

    public async Task<ResultModel<MessageResultModel>> UpdateUserAddress(Guid id, UserAddressUpdateReqModel userAddressReq, string token)
    {
        try
        {
            Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
            var user = await _userRepo.GetSingle(x => x.Id == userId);

            if (user == null || user.Status.Equals(AccountStatusEnums.Inactive))
            {
                throw new CustomException("You are banned from updating address due to violation of terms!");
            }

            var userAddress = await _userAddressRepo.GetSingle(x => x.Id == id && x.UserId == userId);
            if (userAddress == null)
            {
                throw new CustomException("Address not found!");
            }

            var (latitude, longitude) = await GetCoordinatesFromHereAsync(userAddressReq.Address);

            userAddress = _mapper.Map(userAddressReq, userAddress);
            userAddress.Name = TextConvert.ConvertToUnicodeEscape(userAddressReq.Name);
            userAddress.Phone = userAddressReq.Phone;
            userAddress.UpdatedAt = DateTime.Now;
            userAddress.Ward = TextConvert.ConvertToUnicodeEscape(userAddressReq.Ward);
            userAddress.District = TextConvert.ConvertToUnicodeEscape(userAddressReq.District);
            userAddress.Province = TextConvert.ConvertToUnicodeEscape(userAddressReq.Province);

            if (latitude.HasValue && longitude.HasValue)
            {
                userAddress.Latitude = (decimal)latitude.Value;
                userAddress.Longitude = (decimal)longitude.Value;
            }

            await _userAddressRepo.Update(userAddress);

            return new ResultModel<MessageResultModel>()
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new MessageResultModel()
                {
                    Message = "Address is updated!"
                }
            };
        }
        catch (Exception ex)
        {
            throw new CustomException($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ResultModel<MessageResultModel>> SetDefaultUserAddress(Guid id, string token)
    {
        try
        {
            Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
            var user = await _userRepo.GetSingle(x => x.Id == userId);

            if (user == null || user.Status.Equals(AccountStatusEnums.Inactive))
            {
                throw new CustomException("You are banned from setting default address due to violation of terms!");
            }

            var userAddresses = await _userAddressRepo.GetList(x => x.UserId.Equals(userId));
            var userAddress = await _userAddressRepo.GetSingle(x => x.Id == id && x.UserId == userId);
            if (userAddress == null)
            {
                throw new CustomException("Address not found!");
            }

            foreach (var address in userAddresses)
            {
                address.Status = address.Id == id ? UserAddressEnums.Default.ToString() : UserAddressEnums.Active.ToString();
                await _userAddressRepo.Update(address);
            }

            return new ResultModel<MessageResultModel>()
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new MessageResultModel()
                {
                    Message = "Default address is set!"
                }
            };
        }
        catch (Exception ex)
        {
            throw new CustomException($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ResultModel<MessageResultModel>> DeleteUserAddress(Guid id, string token)
    {
        try
        {
            Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
            var user = await _userRepo.GetSingle(x => x.Id == userId);

            if (user == null || user.Status.Equals(AccountStatusEnums.Inactive))
            {
                throw new CustomException("You are banned from deleting address due to violation of terms!");
            }

            var userAddress = await _userAddressRepo.GetSingle(x => x.Id == id && x.UserId == userId);
            if (userAddress == null)
            {
                throw new CustomException("Address not found!");
            }

            await _userAddressRepo.Delete(userAddress);

            return new ResultModel<MessageResultModel>()
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new MessageResultModel()
                {
                    Message = "Address is deleted!"
                }
            };
        }
        catch (Exception ex)
        {
            throw new CustomException($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ResultModel<ListDataResultModel<ListUserAddressResModel>>> GetUserAddress(string token)
    {
        try
        {
            Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
            var user = await _userRepo.GetSingle(x => x.Id == userId);

            if (user == null || user.Status.Equals(AccountStatusEnums.Inactive))
            {
                throw new CustomException("You are banned from getting address due to violation of terms!");
            }

            var userAddresses = await _userAddressRepo.GetList(x => x.UserId.Equals(userId));

            var result = new ListDataResultModel<ListUserAddressResModel>();
            result.Data = userAddresses.Select(x => new ListUserAddressResModel
            {
                Id = x.Id,
                Name = TextConvert.ConvertFromUnicodeEscape(x.Name),
                Phone = x.Phone,
                Address = TextConvert.ConvertFromUnicodeEscape(x.Address),
                Ward = TextConvert.ConvertFromUnicodeEscape(x.Ward),
                District = TextConvert.ConvertFromUnicodeEscape(x.District),
                Province = TextConvert.ConvertFromUnicodeEscape(x.Province),
                IsDefault = x.Status.Equals(UserAddressEnums.Default.ToString())
            }).ToList();

            return new ResultModel<ListDataResultModel<ListUserAddressResModel>>()
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = result
            };
        }
        catch (Exception ex)
        {
            throw new CustomException($"An error occurred: {ex.Message}");
        }
    }
}

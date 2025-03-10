using System.Net;
using AutoMapper;
using HMES.Business.Utilities.Authentication;
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
}

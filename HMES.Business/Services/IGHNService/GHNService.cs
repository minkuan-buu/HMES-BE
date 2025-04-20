using System.Net;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.ResponseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HMES.Business.Services.GHNService
{
    public class GHNService : IGHNService
    {
        private readonly HttpClient _httpClient;

        public GHNService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/")
            };
            _httpClient.DefaultRequestHeaders.Add("Token", Environment.GetEnvironmentVariable("GHN_TOKEN"));
        }

        public async Task<ResultModel<List<ProvinceResponse>>> GetProvince()
        {
            try
            {
                var response = await SendGetRequest("province");

                var jsonObject = JsonConvert.DeserializeObject<JObject>(response);
                var data = jsonObject["data"]?.ToObject<List<ProvinceResponse>>();

                return new ResultModel<List<ProvinceResponse>>
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = data!
                };
            }
            catch (Exception ex)
            {
                throw new CustomException($"An error occurred while fetching provinces: {ex.Message}");
            }
        }

        public async Task<ResultModel<List<DistrictResponse>>> GetDistrict(string provinceId)
        {
            try
            {
                var response = await SendGetRequest($"district?province_id={provinceId}");
                var jsonObject = JsonConvert.DeserializeObject<JObject>(response);
                var data = jsonObject["data"]?.ToObject<List<DistrictResponse>>();

                return new ResultModel<List<DistrictResponse>>
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = data!
                };
            }
            catch (Exception ex)
            {
                throw new CustomException($"An error occurred while fetching districts: {ex}");
            }
        }

        public async Task<ResultModel<List<WardResponse>>> GetWard(string districtId)
        {
            try
            {
                var response = await SendGetRequest($"ward?district_id={districtId}");
                var jsonObject = JsonConvert.DeserializeObject<JObject>(response);
                var data = jsonObject["data"]?.ToObject<List<WardResponse>>();
                return new ResultModel<List<WardResponse>>
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = data!
                };
            }
            catch (Exception ex)
            {
                throw new CustomException($"An error occurred while fetching wards: {ex}");
            }
        }

        private async Task<string> SendGetRequest(string url)
        {
            var response = await _httpClient.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
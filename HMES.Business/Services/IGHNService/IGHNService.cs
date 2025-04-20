using HMES.Data.DTO.ResponseModel;

namespace HMES.Business.Services.GHNService
{
    public interface IGHNService
    {
        Task<ResultModel<List<ProvinceResponse>>> GetProvince();
        Task<ResultModel<List<DistrictResponse>>> GetDistrict(string provinceId);
        Task<ResultModel<List<WardResponse>>> GetWard(string districtId);
    }
}
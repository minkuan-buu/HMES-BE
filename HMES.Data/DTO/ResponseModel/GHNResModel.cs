namespace HMES.Data.DTO.ResponseModel
{
    public class ProvinceResponse
    {
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; } = null!;
    }

    public class DistrictResponse
    {
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }
        public string DistrictName { get; set; } = null!;
    }

    public class WardResponse
    {
        public string WardCode { get; set; } = null!;
        public string WardName { get; set; } = null!;
    }
}
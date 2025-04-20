using HMES.Business.Services.DeviceItemServices;
using HMES.Business.Services.DeviceServices;
using HMES.Business.Services.GHNService;
using HMES.Business.Services.UserServices;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers
{
        [Route("api/ghn")]
        [ApiController]
        public class GHNController : ControllerBase
        {
                private readonly IGHNService _ghnServices;

                public GHNController(IGHNService ghnServices)
                {
                        _ghnServices = ghnServices;
                }

                [HttpGet("province")]
                public async Task<IActionResult> GetProvince()
                {
                        var result = await _ghnServices.GetProvince();
                        return Ok(result);
                }

                [HttpGet("district")]
                public async Task<IActionResult> GetDistrict([FromQuery] string provinceId)
                {
                        var result = await _ghnServices.GetDistrict(provinceId);
                        return Ok(result);
                }

                [HttpGet("ward")]
                public async Task<IActionResult> GetWard([FromQuery] string districtId)
                {
                        var result = await _ghnServices.GetWard(districtId);
                        return Ok(result);
                }
        }
}

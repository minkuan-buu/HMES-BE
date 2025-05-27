using HMES.Business.Services.DeviceItemServices;
using HMES.Business.Services.DeviceServices;
using HMES.Business.Services.PhaseServices;
using HMES.Business.Services.PlantServices;
using HMES.Business.Services.TargetValueServices;
using HMES.Business.Services.UserServices;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers
{
        [Route("api/device")]
        [ApiController]
        public class DeviceController : ControllerBase
        {
                private readonly IDeviceServices _deviceServices;
                private readonly IDeviceItemServices _deviceItemServices;
                private readonly IPhaseServices _phaseServices;
                private readonly IPlantServices _plantServices;
                private readonly ITargetValueServices _targetValueServices;

                public DeviceController(ITargetValueServices targetValueServices, IPhaseServices phaseServices, IDeviceServices deviceServices, IDeviceItemServices deviceItemServices, IPlantServices plantServices)
                {
                        _deviceServices = deviceServices;
                        _deviceItemServices = deviceItemServices;
                        _phaseServices = phaseServices;
                        _plantServices = plantServices;
                        _targetValueServices = targetValueServices;
                }

                [HttpPost]
                [Authorize(AuthenticationSchemes = "HMESAuthentication")]
                public async Task<IActionResult> CreateDevice([FromForm] DeviceCreateReqModel device)
                {
                        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                        var result = await _deviceServices.CreateDevices(device, token);
                        return Ok(result);
                }

                [HttpGet("{Id}")]
                public async Task<IActionResult> GetDeviceDetailById(Guid Id)
                {
                        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                        var result = await _deviceServices.GetDeviceDetailById(Id);
                        return Ok(result);
                }

                [HttpDelete("{Id}")]
                [Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
                public async Task<IActionResult> DeleteDeviceById(Guid Id)
                {
                        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                        var result = await _deviceServices.DeleteDeviceById(Id, token);
                        return Ok(result);
                }

                [HttpGet]
                public async Task<IActionResult> GetListDevice()
                {
                        var result = await _deviceServices.GetListDevice();
                        return Ok(result);
                }

                //========================================================================
                // Device Item of device owner endpoint

                [HttpGet("me")]
                [Authorize(AuthenticationSchemes = "HMESAuthentication")]
                public async Task<IActionResult> GetMyDevices()
                {
                        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                        var result = await _deviceServices.GetListDeviceByUserId(token);
                        return Ok(result);
                }

                [HttpPut("set-plant")]
                [Authorize(AuthenticationSchemes = "HMESAuthentication")]
                public async Task<IActionResult> SetPlantForDeviceDevice([FromBody] SetPlantReqModel model)
                {
                        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                        var result = await _deviceItemServices.SetPlantForDevice(model.DeviceItemId, model.PlantId, token);
                        return Ok(result);
                }

                [HttpGet("phase/{plantId}")]
                public async Task<IActionResult> GetPhasesOfPlantAsync(Guid plantId)
                {
                        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                        var result = await _phaseServices.GetAllPhasesIncludeUserAsync(plantId, token);
                        return Ok(result);
                }

                [HttpPut("set-phase")]
                [Authorize(AuthenticationSchemes = "HMESAuthentication")]
                public async Task<IActionResult> SetPhaseForDeviceDevice([FromBody] SetPhaseReqModel model)
                {
                        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                        var result = await _deviceItemServices.SetPhaseForDevice(model.DeviceItemId, model.phaseId, token);
                        return Ok(result);
                }

                [HttpPost("{plantId}/target/{targetId}/phase/{phaseId}")]
                public async Task<IActionResult> SetValueForCustomPhase(Guid plantId, Guid targetId, Guid phaseId)
                {
                        var result = await _plantServices.SetValueForCustomPhase(plantId, targetId, phaseId);
                        return Ok(result);
                }

                // Use for creating and update (name)
                [HttpPost("init-custom-phase")]
                [Authorize(AuthenticationSchemes = "HMESAuthentication")]
                public async Task<IActionResult> CreatePhase([FromBody] AddNewPhaseDto phaseDto)
                {
                        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                        var result = await _phaseServices.CreateNewPhaseAsync(phaseDto, token);
                        return Ok(result);
                }

                [HttpPost("{plantId}/phase/{phaseId}")]
                public async Task<IActionResult> SetPhaseForPlant(Guid plantId, Guid phaseId)
                {
                        var result = await _phaseServices.SetPhaseForPlant(plantId, phaseId);
                        return Ok(result);
                }
                [HttpPost("set-value")]
                public async Task<IActionResult> SetValueForCustomPhase([FromBody] SetValueReqModel model)
                {
                        var result = await _targetValueServices.SetValueForDevice(model);
                        return Ok(result);
                }

                [HttpPut("update-value")]
                public async Task<IActionResult> UpdateValueForCustomPhase([FromBody] SetValueReqModel model)
                {
                        var result = await _targetValueServices.UpdateValueForDevice(model);
                        return Ok(result);
                }

                //=============================================================================

                [HttpPut("{Id}")]
                [Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
                public async Task<IActionResult> UpdateDevice([FromForm] DeviceUpdateReqModel model, Guid Id)
                {
                        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                        var result = await _deviceServices.UpdateDevice(model, token, Id);
                        return Ok(result);
                }

        }
}

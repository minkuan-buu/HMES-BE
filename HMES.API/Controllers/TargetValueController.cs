using HMES.Business.Services.TargetValueServices;
using HMES.Data.DTO.RequestModel;
using HMES.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers;

[Route("api/target-value")]
[ApiController]
public class TargetValueController : ControllerBase
{
    
    private readonly ITargetValueServices _targetValueServices;
    
    public TargetValueController(ITargetValueServices targetValueServices)
    {
        _targetValueServices = targetValueServices;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllTargetValuesAsync(
        [FromQuery] ValueTypeEnums? type, 
        [FromQuery] decimal? minValue, [FromQuery] decimal? maxValue, 
        [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _targetValueServices.GetAllTargetValuesAsync(type.ToString(), minValue, maxValue, pageIndex, pageSize);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var result = await _targetValueServices.GetByIdAsync(id);
        return Ok(result);
    }
    
    [HttpPost]
    //[Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> CreateTargetValueAsync([FromForm] TargetReqModel targetReqModel)
    {
        var result = await _targetValueServices.CreateTargetValueAsync(targetReqModel);
        return Ok(result);
    }
    
    [HttpPut("{id}")]
    //[Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> UpdateTargetValueAsync(Guid id, [FromForm] TargetReqModel targetReqModel)
    {
        var result = await _targetValueServices.UpdateTargetValueAsync(id, targetReqModel);
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    //[Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> DeleteTargetValueAsync(Guid id)
    {
        var result = await _targetValueServices.DeleteTargetValueAsync(id);
        return Ok(result);
    }
    
    [HttpGet("{plantId}/{phaseId}")]
    public async Task<IActionResult> GetValueForDeviceItem(Guid plantId, Guid phaseId)
    {
        var result = await _targetValueServices.GetValueByPlantAndPhase(plantId, phaseId);
        return Ok(result);
    }
    
    
}
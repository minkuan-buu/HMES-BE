using HMES.Business.Services.TargetValueServices;
using HMES.Data.DTO.RequestModel;
using HMES.Data.Enums;
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
    public async Task<IActionResult> CreateTargetValueAsync([FromBody] TargetReqModel targetReqModel)
    {
        var result = await _targetValueServices.CreateTargetValueAsync(targetReqModel);
        return Ok(result);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTargetValueAsync(Guid id, [FromForm] TargetReqModel targetReqModel)
    {
        var result = await _targetValueServices.UpdateTargetValueAsync(id, targetReqModel);
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTargetValueAsync(Guid id)
    {
        var result = await _targetValueServices.DeleteTargetValueAsync(id);
        return Ok(result);
    }
    
    
    
    
}
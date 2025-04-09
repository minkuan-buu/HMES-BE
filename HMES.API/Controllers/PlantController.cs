using HMES.Business.Services.PlantServices;
using HMES.Data.DTO.RequestModel;
using HMES.Data.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers;
[Route("api/plant")]
[ApiController]
public class PlantController: ControllerBase
{
    
    private readonly IPlantServices _plantServices;

    public PlantController(IPlantServices plantServices)
    {
        _plantServices = plantServices;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPlantsAsync(
        string? keyword, 
        PlantStatusEnums? status, 
        [FromQuery] int pageIndex = 1, 
        [FromQuery] int pageSize = 10)
    {
        var result = await _plantServices.GetAllPlantsAsync(keyword, status.ToString(), pageIndex, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var result = await _plantServices.GetByIdAsync(id);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddPlant([FromForm] PlantReqModel plantDto)
    {
        var result = await _plantServices.CreatePlantAsync(plantDto);
        return Ok(result);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlant(
        Guid id,
        [FromForm] PlantReqModel plantDto)
    {
        var result = await _plantServices.UpdatePlantAsync(id, plantDto);
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlant(Guid id)
    {
        var result = await _plantServices.DeletePlantAsync(id);
        return Ok(result);
    }
    
    [HttpPost("{plantId}/target/{targetId}")]
    public async Task<IActionResult> SetValueForPlant(Guid plantId, Guid targetId)
    {
        var result = await _plantServices.SetValueForPlant(plantId, targetId);
        return Ok(result);
    }
    
    [HttpDelete("{plantId}/target/{targetId}")]
    public async Task<IActionResult> RemoveValueForPlant(Guid plantId, Guid targetId)
    {
        var result = await _plantServices.RemoveValueForPlant(plantId, targetId);
        return Ok(result);
    }
    
    [HttpPut("target/change")]
    public async Task<IActionResult> UpdateValueForPlant(
        [FromBody] ChangeTargetReqModel changeTargetReqModel)
    {
        var result = await _plantServices.UpdateValueForPlant(changeTargetReqModel.PlantId,changeTargetReqModel.TargetId,changeTargetReqModel.NewTargetId);
        return Ok(result);
    }
    
    [HttpGet("not-set-value/{type}")]
    public async Task<IActionResult> GetPlantNotSetValueOfType(string type)
    {
        var result = await _plantServices.GetPlantNotSetValueOfType(type);
        return Ok(result);
    }
    
    
    
}
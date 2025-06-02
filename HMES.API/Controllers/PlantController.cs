using HMES.Business.Services.PlantServices;
using HMES.Data.DTO.RequestModel;
using HMES.Data.Enums;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> AddPlant([FromForm] PlantReqModel plantDto)
    {
        var result = await _plantServices.CreatePlantAsync(plantDto);
        return Ok(result);
    }
    
    
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> UpdatePlant(
        Guid id,
        [FromForm] PlantReqModel plantDto)
    {
        var result = await _plantServices.UpdatePlantAsync(id, plantDto);
        return Ok(result);
    }
    
    
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> DeletePlant(Guid id)
    {
        var result = await _plantServices.DeletePlantAsync(id);
        return Ok(result);
    }
    
    [HttpPost("{plantId}/target/{targetId}/phase/{phaseId}")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> SetValueForPlant(Guid plantId, Guid targetId,Guid phaseId)
    {
        var result = await _plantServices.SetValueForPlant(plantId, targetId,phaseId);
        return Ok(result);
    }
    
    [HttpDelete("{plantId}/target/{targetId}/phase/{phaseId}")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> RemoveValueForPlant(Guid plantId, Guid targetId,Guid phaseId)
    {
        var result = await _plantServices.RemoveValueForPlant(plantId, targetId,phaseId);
        return Ok(result);
    }
    
    
    [HttpPut("target/change")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> UpdateValueForPlant(
        [FromBody] ChangeTargetReqModel changeTargetReqModel)
    {
        var result = await _plantServices.UpdateValueForPlant(changeTargetReqModel.PlantId,changeTargetReqModel.TargetId,changeTargetReqModel.NewTargetId,changeTargetReqModel.PhaseId);
        return Ok(result);
    }
    
    [HttpGet("not-set-value/{type}")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> GetPlantNotSetValueOfType(string type)
    {
        var result = await _plantServices.GetPlantNotSetValueOfType(type);
        return Ok(result);
    }
    [HttpPost("{plantId}/set-phase/{phaseId}")] 
    [Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> SetPhaseForPlant(Guid plantId, Guid phaseId)
    {
        var result = await _plantServices.SetPhaseForPlant(plantId, phaseId);
        return Ok(result);
    }
    [HttpDelete("{plantId}/remove-phase/{phaseId}")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> RemovePhaseForPlant(Guid plantId, Guid phaseId)
    {
        var result = await _plantServices.RemovePhaseForPlant(plantId, phaseId);
        return Ok(result);
    }
    
    
    
    
    
}
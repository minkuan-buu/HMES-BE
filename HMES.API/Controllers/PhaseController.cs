using HMES.Business.Services.PhaseServices;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers;

[Route("api/phase")]
[ApiController]
public class PhaseController : ControllerBase
{

    private readonly IPhaseServices _phaseServices;


    public PhaseController(IPhaseServices phaseServices)
    {
        _phaseServices = phaseServices;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPhasesAsync()
    {
        var result = await _phaseServices.GetAllPhasesAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var result = await _phaseServices.GetPhaseByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    //[Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> CreatePhase([FromForm] AddNewPhaseDto phaseDto)
    {
        var result = await _phaseServices.CreateNewPhaseAsync(phaseDto, null);
        return Ok(result);
    }

    [HttpPut("{id}")]
    //[Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Admin")]
    public async Task<IActionResult> UpdatePhase(
        Guid id,
        [FromForm] AddNewPhaseDto phaseDto)
    {
        var result = await _phaseServices.UpdatePhaseAsync(id, phaseDto, null);
        return Ok(result);
    }


}
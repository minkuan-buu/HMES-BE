using HMES.Business.Services.TicketServices;
using HMES.Data.DTO.RequestModel;
using HMES.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers;

[Route("api/ticket")]
[ApiController]
public class TicketController : ControllerBase
{
    private readonly ITicketServices _ticketServices;

    public TicketController(ITicketServices ticketServices)
    {
        _ticketServices = ticketServices;
    }

    // GET: api/ticket
    [HttpGet]
    [Authorize(AuthenticationSchemes = "HMESAuthentication")]
    public async Task<IActionResult> GetAllTickets(
        [FromQuery] string? keyword,
        [FromQuery] TicketTypeEnums? type,
        [FromQuery] TicketStatusEnums? status,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        var result = await _ticketServices.GetAllTickets(keyword, type.ToString(), status.ToString(), pageIndex, pageSize, token);
        return Ok(result);
    }

    // GET: api/ticket/assigned
    [HttpGet("assigned")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication")]
    public async Task<IActionResult> GetTicketsByToken(
        [FromQuery] string? keyword,
        [FromQuery] TicketTypeEnums? type,
        [FromQuery] TicketStatusEnums? status,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        var result = await _ticketServices.GetTicketsWasAssignedByMe(keyword, type.ToString(), status.ToString(), token, pageIndex, pageSize);
        return Ok(result);
    }

    // GET: api/ticket/{id}
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication")]
    public async Task<IActionResult> GetTicketById(string id)
    {
        var result = await _ticketServices.GetTicketById(id);
        return Ok(result);
    }

    // POST: api/ticket
    [HttpPost]
    [Authorize(AuthenticationSchemes = "HMESAuthentication")]
    public async Task<IActionResult> AddTicket([FromForm] TicketCreateDto ticketDto)
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        var result = await _ticketServices.AddTicket(ticketDto, token);
        return Ok(result);
    }

    // POST: api/ticket/response
    [HttpPost("response")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication")]
    public async Task<IActionResult> ResponseTicket([FromForm] TicketResponseDto ticketDto)
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        var result = await _ticketServices.ResponseTicket(ticketDto,token);
        return Ok(result);
    }

    // PUT: api/ticket/assign/{ticketId}
    [HttpPut("assign/{ticketId}")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication")]
    public async Task<IActionResult> AssignTicket(string ticketId)
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        var result = await _ticketServices.AssignTicket(ticketId, token);
        return Ok(result);
    }

    // PUT: api/ticket/status/{ticketId}
    [HttpPut("status/{ticketId}")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication")]
    public async Task<IActionResult> ChangeTicketStatus(
        string ticketId,
        [FromForm] TicketStatusEnums status)
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        var result = await _ticketServices.ChangeTicketStatus(ticketId, token, status.ToString());
        return Ok(result);
    }
    
    // PUT: api/ticket/transfer/{ticketId}
    [HttpPut("transfer/{ticketId}")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication")]
    public async Task<IActionResult> TransferTicket(
        Guid ticketId,
        [FromForm] Guid transferTo)
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        var result = await _ticketServices.TransferTicket(ticketId, token, transferTo);
        return Ok(result);
    }
    
    // GET: api/ticket/transfer
    [HttpGet("transfer")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication")]
    public async Task<IActionResult> LoadListRequestTransferTicket(
        [FromQuery] string? keyword,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        var result = await _ticketServices.LoadListRequestTransferTicket(keyword, token, pageIndex, pageSize);
        return Ok(result);
    }
    
    // PUT: api/ticket/transfer/{ticketId}
    [HttpPut("transfer/{ticketId}/decision")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication")]
    public async Task<IActionResult> ManageTransferTicket(
        Guid ticketId,
        [FromForm] bool decision)
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        var result = await _ticketServices.ManageTransferTicket(ticketId, decision, token);
        return Ok(result);
    }
    
    // GET: api/ticket/device/{serial}
    [HttpGet("device/{id}")]
    [Authorize(AuthenticationSchemes = "HMESAuthentication")]       
    public async Task<IActionResult> GetDeviceItemById(string id)
    {
        var result = await _ticketServices.GetDeviceItemById(id);
        return Ok(result);
    }
    
}
using HMES.Business.Services.TicketServices;
using HMES.Data.DTO.RequestModel;
using HMES.Data.Enums;
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
    public async Task<IActionResult> GetAllTickets(
        [FromQuery] string? keyword,
        [FromQuery] TicketTypeEnums type,
        [FromQuery] TicketStatusEnums status,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _ticketServices.GetAllTickets(keyword, type.ToString(), status.ToString(), pageIndex, pageSize);
        return StatusCode(result.StatusCodes, result.Response);
    }

    // GET: api/ticket/assigned
    [HttpGet("assigned")]
    public async Task<IActionResult> GetTicketsByToken(
        [FromQuery] string? keyword,
        [FromQuery] TicketTypeEnums type,
        [FromQuery] TicketStatusEnums status,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        var result = await _ticketServices.GetTicketsWasAssignedByMe(keyword, type.ToString(), status.ToString(), token, pageIndex, pageSize);
        return StatusCode(result.StatusCodes, result.Response);
    }

    // GET: api/ticket/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicketById(string id)
    {
        var result = await _ticketServices.GetTicketById(id);
        return StatusCode(result.StatusCodes, result.Response);
    }

    // POST: api/ticket
    [HttpPost]
    public async Task<IActionResult> AddTicket([FromBody] TicketCreateDto ticketDto)
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        var result = await _ticketServices.AddTicket(ticketDto, token);
        return StatusCode(result.StatusCodes, result.Response);
    }

    // PUT: api/ticket/response
    [HttpPut("response")]
    public async Task<IActionResult> ResponseTicket([FromBody] TicketResponseDto ticketDto)
    {
        var result = await _ticketServices.ResponseTicket(ticketDto);
        return StatusCode(result.StatusCodes, result.Response);
    }

    // PUT: api/ticket/assign/{ticketId}
    [HttpPut("assign/{ticketId}")]
    public async Task<IActionResult> AssignTicket(string ticketId)
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        var result = await _ticketServices.AssignTicket(ticketId, token);
        return StatusCode(result.StatusCodes, result.Response);
    }

    // PUT: api/ticket/status/{ticketId}
    [HttpPut("status/{ticketId}")]
    public async Task<IActionResult> ChangeTicketStatus(
        string ticketId,
        [FromBody] string status)
    {
        var token = Request.Headers.Authorization.ToString().Split(" ")[1];
        var result = await _ticketServices.ChangeTicketStatus(ticketId, token, status);
        return StatusCode(result.StatusCodes, result.Response);
    }
}
using System.Net;
using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.TimeZoneHelper;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.TicketRepositories;
using HMES.Data.Repositories.TicketResponseRepositories;

namespace HMES.Business.Services.TicketServices;

public class TicketServices : ITicketServices
{
    
    private readonly ITicketResponseRepositories _ticketResponseRepositories;
    private readonly ITicketRepositories _ticketRepositories;
    private readonly IMapper _mapper;

    
    public TicketServices(ITicketResponseRepositories ticketResponseRepositories, ITicketRepositories ticketRepositories, IMapper mapper)
    {
        _ticketResponseRepositories = ticketResponseRepositories;
        _ticketRepositories = ticketRepositories;
        _mapper = mapper;

    }


    public async Task<ResultModel<ListDataResultModel<TicketBriefDto>>> GetAllTickets(string? keyword, string? type, string? status, int pageIndex, int pageSize)
    {
        var (tickets, totalItems) = await _ticketRepositories.GetAllTicketsAsync(keyword, type, status, pageIndex, pageSize);
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        
        var result = new ListDataResultModel<TicketBriefDto>
        {
            Data = _mapper.Map<List<TicketBriefDto>>(tickets),
            CurrentPage = pageIndex,
            TotalPages = totalPages,
            TotalItems = totalItems,
            PageSize = pageSize
        };
        return new ResultModel<ListDataResultModel<TicketBriefDto>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
        
    }

    public async Task<ResultModel<ListDataResultModel<TicketBriefDto>>> GetTicketsWasAssignedByMe(string? keyword, string? type, string? status, string token, int pageIndex, int pageSize)
    {
        
        Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
        
        var (tickets, totalItems) = await _ticketRepositories.GetTicketsByTokenAsync(keyword, type, status, userId, pageIndex, pageSize);
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        
        var result = new ListDataResultModel<TicketBriefDto>
        {
            Data = _mapper.Map<List<TicketBriefDto>>(tickets),
            CurrentPage = pageIndex,
            TotalPages = totalPages,
            TotalItems = totalItems,
            PageSize = pageSize
        };
        return new ResultModel<ListDataResultModel<TicketBriefDto>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
    }

    public async Task<ResultModel<DataResultModel<TicketDetailsDto>>> GetTicketById(string id)
    {
        var ticket = await _ticketRepositories.GetByIdAsync(new Guid(id));
        if (ticket == null)
        {
            return new ResultModel<DataResultModel<TicketDetailsDto>>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = null
            };
        }
        var result = new DataResultModel<TicketDetailsDto>
        {
            Data = _mapper.Map<TicketDetailsDto>(ticket)
        };
        return new ResultModel<DataResultModel<TicketDetailsDto>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
    }

    public async Task<ResultModel<DataResultModel<TicketDetailsDto>>> AddTicket(TicketCreateDto ticketDto, string token)
    {
        var userId = new Guid(Authentication.DecodeToken(token, "userid"));
        var ticket = _mapper.Map<Ticket>(ticketDto);
        ticket.UserId = userId;
        await _ticketRepositories.Insert(ticket);
        var result = new DataResultModel<TicketDetailsDto>
        {
            Data = _mapper.Map<TicketDetailsDto>(ticket)
        };
        return new ResultModel<DataResultModel<TicketDetailsDto>>
        {
            StatusCodes = (int)HttpStatusCode.Created,
            Response = result
        };
    }

    public async Task<ResultModel<DataResultModel<TicketDetailsDto>>> ResponseTicket(TicketResponseDto ticketDto)
    {
        var ticket = await _ticketRepositories.GetByIdAsync(ticketDto.TicketId);
        if (ticket == null)
        {
            return new ResultModel<DataResultModel<TicketDetailsDto>>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = null
            };
        }
        var ticketResponse = _mapper.Map<TicketResponse>(ticketDto);
        ticketResponse.CreatedAt = TimeZoneHelper.GetCurrentHoChiMinhTime();
        ticket.TicketResponses.Add(ticketResponse);
        ticket.UpdatedAt = TimeZoneHelper.GetCurrentHoChiMinhTime();
        await _ticketRepositories.Update(ticket);
        var result = new DataResultModel<TicketDetailsDto>
        {
            Data = _mapper.Map<TicketDetailsDto>(ticket)
        };
        return new ResultModel<DataResultModel<TicketDetailsDto>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
    }

    public async Task<ResultModel<MessageResultModel>> AssignTicket(string ticketId, string token)
    {
        Guid technicianId = new Guid(Authentication.DecodeToken(token, "userid"));
        var ticket = await _ticketRepositories.GetByIdAsync(new Guid(ticketId));
        if (ticket == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel
                {
                    Message = "Ticket not found"
                }
            };
        }
        ticket.TeachnicianId = technicianId;
        ticket.Status = TicketStatusEnums.InProgress.ToString();
        ticket.UpdatedAt = TimeZoneHelper.GetCurrentHoChiMinhTime();
        await _ticketRepositories.Update(ticket);
        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel
            {
                Message = "Ticket assigned"
            }
        };
    }

    public async Task<ResultModel<MessageResultModel>> ChangeTicketStatus(string ticketId, string token, string status)
    {
        Guid technicianId = new Guid(Authentication.DecodeToken(token, "userid"));
        var ticket = await _ticketRepositories.GetByIdAsync(new Guid(ticketId));
        if (ticket == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel
                {
                    Message = "Ticket not found"
                }
            };
        }
        ticket.Status = status;
        ticket.IsProcessed = true;
        ticket.UpdatedAt = TimeZoneHelper.GetCurrentHoChiMinhTime();
        await _ticketRepositories.Update(ticket);
        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel
            {
                Message = "Ticket status changed"
            }
        };
    }
}
using System.Net;
using System.Security.Claims;
using AutoMapper;
using HMES.Business.Services.CloudServices;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.TimeZoneHelper;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.DeviceItemsRepositories;
using HMES.Data.Repositories.DeviceRepositories;
using HMES.Data.Repositories.TicketRepositories;
using HMES.Data.Repositories.TicketResponseRepositories;
using Microsoft.IdentityModel.Tokens;

namespace HMES.Business.Services.TicketServices;

public class TicketServices : ITicketServices
{
    
    private readonly ITicketResponseRepositories _ticketResponseRepositories;
    private readonly IDeviceItemsRepositories _deviceItemsRepositories;
    private readonly ITicketRepositories _ticketRepositories;
    private readonly ICloudServices _cloudServices;
    private readonly IMapper _mapper;

    
    public TicketServices(ITicketResponseRepositories ticketResponseRepositories, ICloudServices iCloudServices, ITicketRepositories ticketRepositories, IMapper mapper, IDeviceItemsRepositories deviceItemsRepositories)
    {
        _ticketResponseRepositories = ticketResponseRepositories;
        _ticketRepositories = ticketRepositories;
        _mapper = mapper;
        _cloudServices = iCloudServices;
        _deviceItemsRepositories = deviceItemsRepositories;

    }


    public async Task<ResultModel<ListDataResultModel<TicketBriefDto>>> GetAllTickets(string? keyword, string? type, string? status, int pageIndex, int pageSize, string token)
    {
        
        var role = Authentication.DecodeToken(token, ClaimsIdentity.DefaultRoleClaimType);
        if (role.Equals(RoleEnums.Technician.ToString()))
        {
            status = TicketStatusEnums.Pending.ToString();
        }
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
        
        var ticketDetails = _mapper.Map<TicketDetailsDto>(ticket);
        
        
        var result = new DataResultModel<TicketDetailsDto>
        {
            Data = ticketDetails
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
        if ((ticketDto.DeviceItemId != null && ticketDto.Type == TicketTypeEnums.Technical) ||
            (ticketDto.DeviceItemId == null && ticketDto.Type == TicketTypeEnums.Shopping))
        {
            if (ticketDto.DeviceItemId != null)
            {
                 var device = await _deviceItemsRepositories.GetDeviceItemByDeviceItemIdAndUserId((Guid)ticketDto.DeviceItemId, userId);
                 if(device == null)
                 {
                     return new ResultModel<DataResultModel<TicketDetailsDto>>
                     {
                         StatusCodes = (int)HttpStatusCode.NotFound,
                         Response = null
                     };
                 }
            }
            var ticket = _mapper.Map<Ticket>(ticketDto);
            var filePath = $"ticket/{ticket.Id}/attachments";
            if (!ticketDto.Attachments.IsNullOrEmpty())
            {
                var attachments = await _cloudServices.UploadFile(ticketDto.Attachments, filePath);
                var ticketAttachments = new List<TicketAttachment>();
                ticketAttachments.AddRange(attachments.Select(attachment =>
                    new TicketAttachment { Id = Guid.NewGuid(), TicketId = ticket.Id, Attachment = attachment }));
                ticket.TicketAttachments = ticketAttachments;
            }
            ticket.UserId = userId;
            await _ticketRepositories.Insert(ticket);
            var result = new DataResultModel<TicketDetailsDto> { Data = _mapper.Map<TicketDetailsDto>(ticket) };
            return new ResultModel<DataResultModel<TicketDetailsDto>>
            {
                StatusCodes = (int)HttpStatusCode.Created, Response = result
            };
        }

        return new ResultModel<DataResultModel<TicketDetailsDto>>
        {
            StatusCodes = (int)HttpStatusCode.BadRequest,
            Response = null
        };
    }

    public async Task<ResultModel<DataResultModel<TicketDetailsDto>>> ResponseTicket(TicketResponseDto ticketDto, string token)
    {
        var userId = new Guid(Authentication.DecodeToken(token, "userid"));
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
        var filePath = $"ticket/{ticketResponse.Id}/attachments";
        if (!ticketDto.Attachments.IsNullOrEmpty())
        {
            var attachments = await _cloudServices.UploadFile(ticketDto.Attachments, filePath);
            var ticketAttachments = new List<TicketResponseAttachment>();
            ticketAttachments.AddRange(attachments.Select(attachment =>
                new TicketResponseAttachment { Id = Guid.NewGuid(), TicketResponseId = ticketResponse.Id, Attachment = attachment }));
            ticketResponse.TicketResponseAttachments = ticketAttachments;
        }
        ticketResponse.CreatedAt = TimeZoneHelper.GetCurrentHoChiMinhTime();
        ticketResponse.UserId = userId;
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
        ticket.TechnicianId = technicianId;
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
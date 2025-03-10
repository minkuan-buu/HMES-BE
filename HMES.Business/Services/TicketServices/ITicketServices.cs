using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;

namespace HMES.Business.Services.TicketServices;

public interface ITicketServices
{
    Task<ResultModel<ListDataResultModel<TicketBriefDto>>> GetAllTickets(
        string? keyword,
        string? type,
        string? status,
        int pageIndex, 
        int pageSize);
    
    Task<ResultModel<ListDataResultModel<TicketBriefDto>>> GetTicketsWasAssignedByMe(
        string? keyword,
        string? type,
        string? status,
        string token,
        int pageIndex, 
        int pageSize);
    
    Task<ResultModel<DataResultModel<TicketDetailsDto>>> GetTicketById(string id);
    
    Task<ResultModel<DataResultModel<TicketDetailsDto>>> AddTicket(TicketCreateDto ticketDto, string token);
    
    Task<ResultModel<DataResultModel<TicketDetailsDto>>> ResponseTicket(TicketResponseDto ticketDto);
    
    Task<ResultModel<MessageResultModel>> AssignTicket(string ticketId, string token);
    
    Task<ResultModel<MessageResultModel>> ChangeTicketStatus(string ticketId, string token, string status);
}
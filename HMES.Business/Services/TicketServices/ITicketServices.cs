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
        int pageSize,
        string token);
    
    Task<ResultModel<ListDataResultModel<TicketBriefDto>>> GetTicketsWasAssignedByMe(
        string? keyword,
        string? type,
        string? status,
        string token,
        int pageIndex, 
        int pageSize);
    
    Task<ResultModel<DataResultModel<TicketDetailsDto>>> GetTicketById(string id);
    
    Task<ResultModel<DataResultModel<TicketDetailsDto>>> AddTicket(TicketCreateDto ticketDto, string token);
    
    Task<ResultModel<DataResultModel<TicketDetailsDto>>> ResponseTicket(TicketResponseDto ticketDto, string token);
    
    Task<ResultModel<MessageResultModel>> AssignTicket(string ticketId, string token);
    
    Task<ResultModel<MessageResultModel>> ChangeTicketStatus(string ticketId, string token, string status);
    
    Task<ResultModel<MessageResultModel>> TransferTicket(Guid ticketId, string token, Guid transferTo);
    
    Task<ResultModel<ListDataResultModel<TicketBriefDto>>> LoadListRequestTransferTicket(
        string? keyword,
        string token,
        int pageIndex, 
        int pageSize);
    Task<ResultModel<MessageResultModel>> ManageTransferTicket(Guid ticketId, bool decision, string token);
    
    Task<ResultModel<DataResultModel<TicketDeviceItemDto>>> GetDeviceItemById(string serial);
    
}
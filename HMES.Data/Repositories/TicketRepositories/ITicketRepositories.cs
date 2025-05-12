using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.TicketRepositories;

public interface ITicketRepositories: IGenericRepositories<Ticket>
{
    public Task<(List<Ticket> tickets, int TotalItems)> GetAllTicketsAsync(string? keyword, string? type,
        string? status, int pageIndex, int pageSize);
    public Task<(List<Ticket> tickets, int TotalItems)> GetAllOwnTicketsAsync(string? keyword, string? type,
        string? status, Guid userId, int pageIndex, int pageSize);
    
    public Task<Ticket?> GetByIdAsync(Guid id);
    Task<(List<Ticket> tickets, int totalItems)> GetTicketsByTokenAsync(string? keyword, string? type, string? status, Guid userId, int pageIndex, int pageSize);
    Task<(List<Ticket> tickets, int totalItems)> GetTicketsRequestTransferByTokenAsync(string? keyword, string? type, string? status, Guid userId, int pageIndex, int pageSize);
    
    Task<bool> CheckTicketInPendingOrProgressing(Guid userId, Guid? deviceItemId);
    
}
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.TicketRepositories;

public class TicketRepositories : GenericRepositories<Ticket>, ITicketRepositories
{
    public TicketRepositories(HmesContext context) : base(context)
    {
    }
    
    public async Task<(List<Ticket> tickets, int TotalItems)> GetAllTicketsAsync(string? keyword, string? type, string? status, int pageIndex, int pageSize)
    {
        var query = Context.Tickets
            .Include(t => t.User)
            .Include(t => t.Technician)
            .OrderByDescending(t => t.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(t => t.Description.Contains(keyword) || t.Id.ToString().Contains(keyword.ToUpper()));
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(t => t.Type == type);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status == status);
        }
        
        int totalItems = await query.CountAsync();
        var tickets = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (tickets, totalItems);

    }

    public async Task<(List<Ticket> tickets, int TotalItems)> GetAllOwnTicketsAsync(string? keyword, string? type, string? status, Guid userId, int pageIndex, int pageSize)
    {
        var query = Context.Tickets
            .OrderByDescending(t => t.CreatedAt)
            .Include(t => t.User)
            .Include(t => t.Technician)
            .AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(t => t.Description.Contains(keyword) || t.Id.ToString().Contains(keyword.ToUpper()));
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(t => t.Type == type);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status == status);
        }
        
        query = query.Where(t => t.UserId == userId);
        
        int totalItems = await query.CountAsync();
        var tickets = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (tickets, totalItems);
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
    {
        return await Context.Tickets
            .Include(t => t.User)
            .Include(t => t.TicketResponses)
                .ThenInclude(tr => tr.TicketResponseAttachments)
            .Include(t => t.TicketResponses.OrderBy(tr => tr.CreatedAt))
                .ThenInclude(tr => tr.User) // Include User in TicketResponse
            .Include(t => t.TicketAttachments)
            .Include(t => t.DeviceItem)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<(List<Ticket> tickets, int totalItems)> GetTicketsByTokenAsync(string? keyword, string? type, string? status, Guid userId, int pageIndex, int pageSize)
    {
        var query = Context.Tickets
            .OrderByDescending(t => t.CreatedAt)
            .Include(t => t.User)
            .Include(t => t.Technician).AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(t => t.Description.Contains(keyword) || t.Id.ToString().Contains(keyword.ToUpper()));
        }
        
        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(t => t.Type == type);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status == status);
        }

        query = query.Where(t => t.TechnicianId == userId);
        
        int totalItems = await query.CountAsync();
        var tickets = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (tickets, totalItems);
    }

    public async Task<(List<Ticket> tickets, int totalItems)> GetTicketsRequestTransferByTokenAsync(string? keyword, string? type, string? status, Guid userId, int pageIndex,
        int pageSize)
    {
        var query = Context.Tickets
            .OrderByDescending(t => t.CreatedAt)
            .Include(t => t.User)
            .Include(t => t.Technician)
            .AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(t => t.Description.Contains(keyword) || t.Id.ToString().Contains(keyword.ToUpper()));
        }


        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(t => t.Type == type);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status == status);
        }

        query = query.Where(t => t.TransferTo == userId);
        
        int totalItems = await query.CountAsync();
        var tickets = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (tickets, totalItems);
    }

    public async Task<bool> CheckTicketInPendingOrProgressing(Guid userId, Guid? deviceItemId)
    {
        if (deviceItemId != null)
        {
            var query = Context.Tickets
                .Where(t => t.UserId == userId && t.DeviceItemId == deviceItemId)
                .Where(t => t.Status == "Pending" || t.Status == "InProgress")
                .AsQueryable();
            return await query.AnyAsync();
        }
        else
        {
            var query = Context.Tickets
                .Where(t => t.UserId == userId)
                .Where(t => t.Status == nameof(TicketStatusEnums.Pending) || t.Status == nameof(TicketStatusEnums.InProgress))
                .AsQueryable();
            return await query.AnyAsync();
        }
    }
}
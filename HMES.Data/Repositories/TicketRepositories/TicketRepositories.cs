using HMES.Data.Entities;
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
        var query = Context.Tickets.Include(t => t.User).AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(t => t.Description.Contains(keyword));
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
        var query = Context.Tickets.Include(t => t.User).Include(t => t.Technician)
            .Include(t => t.Technician).AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(t => t.Description.Contains(keyword));
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
            .Include(t=>t.User)
            .Include(t => t.TicketResponses)
            .Include(t => t.TicketAttachments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<(List<Ticket> tickets, int totalItems)> GetTicketsByTokenAsync(string? keyword, string? type, string? status, Guid userId, int pageIndex, int pageSize)
    {
        var query = Context.Tickets.Include(t => t.User).Include(t => t.Technician).AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(t => t.Description.Contains(keyword));
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
            .Include(t => t.User)
            .Include(t => t.Technician)
            .AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(t => t.Description.Contains(keyword));
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
}
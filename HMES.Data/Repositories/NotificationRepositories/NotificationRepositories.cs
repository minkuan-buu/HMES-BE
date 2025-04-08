using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.NotificationRepositories;

public class NotificationRepositories : GenericRepositories<Notification>, INotificationRepositories
{
    public NotificationRepositories(HmesContext context) : base(context)
    {
    }


    public async Task<(List<Notification> notifications, int TotalItems)> GetAllNotificationsAsync(Guid? userId, string? keyword, string? type, bool? isRead, int pageIndex, int pageSize)
    {
        // Create the base query
        var query = Context.Notifications.Where(n => n.UserId == userId);
    
        // Apply filters separately with null checks
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(n => n.Title.Contains(keyword) || n.Message.Contains(keyword));
        }
    
        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(n => n.NotificationType == type);
        }

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead);
        }
        
        // Execute queries separately
        int totalItems;
        try
        {
            totalItems = await query.CountAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Count query failed: {ex.Message}");
            totalItems = 0;
        }
    
        List<Notification> notifications;
        try
        {
            notifications = await query
                .Include(n => n.Sender)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Data query failed: {ex.Message}");
            notifications = new List<Notification>();
        }
    
        return (notifications, totalItems);
    }


    public async Task<Notification?> GetNotificationByIdAsync(Guid notificationId)
    {
        var notification = await Context.Notifications
            .Include(n => n.User)
            .Include(n => n.Sender)
            .FirstOrDefaultAsync(x => x.Id == notificationId);
        return notification;
    }
}
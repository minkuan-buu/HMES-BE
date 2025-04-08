using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.NotificationRepositories;

public interface INotificationRepositories : IGenericRepositories<Notification>
{
    Task<(List<Notification> notifications, int TotalItems)> GetAllNotificationsAsync(Guid? userId, string? keyword, string? type,
        bool? isRead, int pageIndex, int pageSize);
    
    Task<Notification?> GetNotificationByIdAsync(Guid notificationId);
    
}
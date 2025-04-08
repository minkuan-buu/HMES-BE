using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;

namespace HMES.Business.Services.NotificationServices;

public interface INotificationServices
{
    Task<ResultModel<ListDataResultModel<NotificationResModel>>> GetAllNotificationAsync(
        string token,
        int pageIndex,
        int pageSize,
        string? keyword,
        string? type,
        bool? isRead);
    
    Task<ResultModel<MessageResultModel>> CreateNotificationAsync(string token, NotificationReqModel notificationReqModel);
    
    Task<ResultModel<MessageResultModel>> ReadNotificationAsync(string token, Guid id);
    
}
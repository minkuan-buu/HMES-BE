using System.Net;
using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.TimeZoneHelper;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Repositories.NotificationRepositories;

namespace HMES.Business.Services.NotificationServices;

public class NotificationServices : INotificationServices
{
    
    private readonly INotificationRepositories _notificationRepositories;
    private readonly IMqttService _mqttService;
    private readonly IMapper _mapper;
    
    
    public NotificationServices(INotificationRepositories notificationRepositories, IMapper mapper, IMqttService mqttService)
    {
        _notificationRepositories = notificationRepositories;
        _mapper = mapper;
        _mqttService = mqttService;
    }   
    
    
    public async Task<ResultModel<ListDataResultModel<NotificationResModel>>> GetAllNotificationAsync(string token, int pageIndex, int pageSize, string? keyword, string? type, bool? isRead)
    {
        
        Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
        
        var (notifications, totalItems) = await _notificationRepositories.GetAllNotificationsAsync(userId, keyword, type, isRead, pageIndex, pageSize);

        var notificationResModels = _mapper.Map<List<NotificationResModel>>(notifications);

        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        
        var result = new ListDataResultModel<NotificationResModel>
        {
            Data = _mapper.Map<List<NotificationResModel>>(notificationResModels),
            CurrentPage = pageIndex,
            TotalPages = totalPages,
            TotalItems = totalItems,
            PageSize = pageSize
        };
        return new ResultModel<ListDataResultModel<NotificationResModel>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
        
    }

    public async Task<ResultModel<MessageResultModel>> CreateNotificationAsync(string token, NotificationReqModel notificationReqModel)
    {
        Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));

        var notification = _mapper.Map<Notification>(notificationReqModel);
        notification.UserId = userId;

        await _notificationRepositories.Insert(notification);

        await PublishNotificationAsync(notification);
        
        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.Created,
            Response = new MessageResultModel
            {
                Message = "Notification created successfully"
            }
        };
        
        
        
    }

    public async Task<ResultModel<MessageResultModel>> ReadNotificationAsync(string token, Guid id)
    {
        Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
        
        var notification = await _notificationRepositories.GetNotificationByIdAsync(id);
        
        if (notification == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel
                {
                    Message = "Notification not found"
                }
            };
        }

        if (notification.UserId != userId)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.BadRequest,
                Response = new MessageResultModel
                {
                    Message = "Notification not belong to this user"
                }
            };
        }

        if ((bool)notification.IsRead)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new MessageResultModel
                {
                    Message = "Notification already read"
                }
            };
        }

        notification.IsRead = true;
        notification.ReadAt = TimeZoneHelper.GetCurrentHoChiMinhTime();
        await _notificationRepositories.Update(notification);

        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel
            {
                Message = "Notification marked as read"
            }
        };
        
    }
    
    private async Task PublishNotificationAsync(Notification notification)
    {
        try
        {
            //Topic
            string topic = $"push/notification/{notification.UserId}";
    
            //Payload 
            var notificationPayload = new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                Type = notification.NotificationType,
                notification.CreatedAt,
                notification.IsRead
            };
    
            // Publish
            await _mqttService.PublishAsync(topic, notificationPayload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error publishing notification: {ex.Message}");
        }
    }
}
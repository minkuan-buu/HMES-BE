using HMES.Business.Services.NotificationServices;
using HMES.Data.DTO.RequestModel;
using HMES.Data.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers;

[Route("api/notify")]
[ApiController]
public class NotificationController : ControllerBase
{
    
    private readonly INotificationServices _notificationServices;
    
    public NotificationController(INotificationServices notificationServices)
    {
        _notificationServices = notificationServices;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllNotification(bool? isRead,
        NotificationTypeEnums? type = null,
        string? keyword = null, int pageIndex = 1, int pageSize = 10)
    {
        
        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
        var result = await _notificationServices.GetAllNotificationAsync(token, pageIndex, pageSize, keyword, type?.ToString(), isRead);
        return Ok(result);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> ReadNotification(Guid id)
    {
        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
        var result = await _notificationServices.ReadNotificationAsync(token, id);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] NotificationReqModel notificationReqModel)
    {
        var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
        var result = await _notificationServices.CreateNotificationAsync(token, notificationReqModel);
        return Ok(result);
    }
    
}
namespace HMES.Data.DTO.RequestModel;

public class NotificationReqModel
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid? SenderId { get; set; }
    public Guid? ReceiverId { get; set; }
    public Guid? ReferenceId { get; set; }
    
}
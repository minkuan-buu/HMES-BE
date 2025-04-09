namespace HMES.Data.DTO.ResponseModel;

public class NotificationResModel
{
    
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? SenderName { get; set; }
    public string? ReceiverName { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
    
}
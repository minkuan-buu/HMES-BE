namespace HMES.Data.DTO.ResponseModel;

public class TicketResModel
{
    
}

public class TicketBriefDto
{
    public string Id { get; set; } = null!;
    public string UserFullName { get; set; } = null!;
    public string BriefDescription { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string CreatedBy { get; set; } = null!;
    public string? HandledBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TicketDetailsDto
{
    public string Id { get; set; } = null!;
    public string UserFullName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Type { get; set; } = null!;
    public Guid? DeviceItemId { get; set; }
    public String? DeviceItemSerial { get; set; }
    public List<string> Attachments { get; set; } = new List<string>();
    public string Status { get; set; } = null!;
    public string CreatedBy { get; set; } = null!;
    public bool IsProcessed { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public List<TicketResponseDetailsDto> TicketResponses { get; set; } = new List<TicketResponseDetailsDto>();
}

public class TicketResponseDetailsDto
{
    public Guid Id { get; set; } 
    public string Message { get; set; } = null!;
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public List<string> Attachments { get; set; } = new List<string>();
    
    
}

using HMES.Data.Enums;

namespace HMES.Data.DTO.RequestModel;

public class TicketReqModel
{
    
}

public class TicketCreateDto
{
    public string Description { get; set; } = null!;
    public string Type { get; set; } = null!;
    public List<string> Attachments { get; set; } = new List<string>();
}

public class TicketResponseDto
{
    public Guid TicketId { get; set; } = Guid.Empty;
    public string Message { get; set; } = null!;
    public string Type { get; set; } = null!;
    public List<string> Attachments { get; set; } = new List<string>();
}

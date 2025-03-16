using HMES.Data.Enums;
using Microsoft.AspNetCore.Http;

namespace HMES.Data.DTO.RequestModel;

public class TicketReqModel
{
    
}

public class TicketCreateDto
{
    public string Description { get; set; } = null!;
    public Guid? DeviceItemId { get; set; }
    public TicketTypeEnums Type { get; set; } = TicketTypeEnums.Shopping;
    public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();
}

public class TicketResponseDto
{
    public Guid TicketId { get; set; } = Guid.Empty;
    public string Message { get; set; } = null!;
    public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();
}

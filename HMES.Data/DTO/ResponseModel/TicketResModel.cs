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
    public DateTime CreatedAt { get; set; }
}

public class TicketDetailsDto
{
    public string Id { get; set; } = null!;
    public string UserFullName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Type { get; set; } = null!;
    public List<string> Attachments { get; set; } = new List<string>();
    public string Status { get; set; } = null!;
    public string CreatedBy { get; set; } = null!;
    public bool IsProcessed { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class Ticket
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? DeviceItemId { get; set; }

    public Guid? TechnicianId { get; set; }

    public string? Type { get; set; }

    public string Description { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool IsProcessed { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? TransferTo { get; set; }

    public virtual DeviceItem? DeviceItem { get; set; }

    public virtual User? Technician { get; set; }

    public virtual ICollection<TicketAttachment> TicketAttachments { get; set; } = new List<TicketAttachment>();

    public virtual ICollection<TicketResponse> TicketResponses { get; set; } = new List<TicketResponse>();

    public virtual User? TransferToNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}

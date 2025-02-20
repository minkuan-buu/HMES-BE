using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class TicketResponse
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid UserId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual ICollection<TicketResponseAttachment> TicketResponseAttachments { get; set; } = new List<TicketResponseAttachment>();
}

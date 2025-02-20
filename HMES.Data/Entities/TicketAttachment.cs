using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class TicketAttachment
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public string Attachment { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;
}

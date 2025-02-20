using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class TicketResponseAttachment
{
    public Guid Id { get; set; }

    public Guid TicketResponseId { get; set; }

    public string Attachment { get; set; } = null!;

    public virtual TicketResponse TicketResponse { get; set; } = null!;
}

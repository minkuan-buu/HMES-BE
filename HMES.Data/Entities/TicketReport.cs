using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class TicketReport
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? Type { get; set; }

    public string Description { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool IsProcessed { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User IdNavigation { get; set; } = null!;
}

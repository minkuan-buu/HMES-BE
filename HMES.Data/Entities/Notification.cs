using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class Notification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? SenderId { get; set; }

    public Guid? ReferenceId { get; set; }

    public string? Title { get; set; }

    public string? Message { get; set; }

    public bool? IsRead { get; set; }

    public string? NotificationType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual User? Sender { get; set; }

    public virtual User User { get; set; } = null!;
}

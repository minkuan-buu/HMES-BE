using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class Transaction
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public int OrderPaymentRefId { get; set; }

    public string? PaymentLinkId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? FinishedTransactionAt { get; set; }

    public string? TransactionReference { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class ProductAttachment
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string Attachment { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class UserToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string RefreshToken { get; set; } = null!;

    public string AccesToken { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class UserAddress
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal? Longitude { get; set; }

    public decimal? Latitude { get; set; }

    public string Ward { get; set; } = null!;

    public string District { get; set; } = null!;

    public string Province { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; } = null!;
}

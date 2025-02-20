﻿using System;
using System.Collections.Generic;

namespace HMES.Data.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public byte[] Password { get; set; } = null!;

    public byte[] Salt { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? Attachment { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Ticket? TicketIdNavigation { get; set; }

    public virtual ICollection<Ticket> TicketTeachnicians { get; set; } = new List<Ticket>();

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
}

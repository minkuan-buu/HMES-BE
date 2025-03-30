using System;
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

    public virtual ICollection<DeviceItem> DeviceItems { get; set; } = new List<DeviceItem>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Otp> Otps { get; set; } = new List<Otp>();

    public virtual ICollection<TicketResponse> TicketResponses { get; set; } = new List<TicketResponse>();

    public virtual ICollection<Ticket> TicketTechnicians { get; set; } = new List<Ticket>();

    public virtual ICollection<Ticket> TicketTransferToNavigations { get; set; } = new List<Ticket>();

    public virtual ICollection<Ticket> TicketUsers { get; set; } = new List<Ticket>();

    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
}

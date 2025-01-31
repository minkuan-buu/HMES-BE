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

    public DateTime? UpdateAt { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<CropHydroponicDevice> CropHydroponicDevices { get; set; } = new List<CropHydroponicDevice>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<RecNutrition> RecNutritions { get; set; } = new List<RecNutrition>();

    public virtual TicketReport? TicketReport { get; set; }

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
}

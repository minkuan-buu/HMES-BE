﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Entities;

public partial class HmesContext : DbContext
{
    public HmesContext(DbContextOptions<HmesContext> options)
        : base(options)
    {
    }

    public HmesContext()
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<NutritionReport> NutritionReports { get; set; }

    public virtual DbSet<NutritionReportDetail> NutritionReportDetails { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<Plant> Plants { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<TargetOfPlant> TargetOfPlants { get; set; }

    public virtual DbSet<TargetValue> TargetValues { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketAttachment> TicketAttachments { get; set; }

    public virtual DbSet<TicketResponse> TicketResponses { get; set; }

    public virtual DbSet<TicketResponseAttachment> TicketResponseAttachments { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAddress> UserAddresses { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cart__3214EC07358B8029");

            entity.ToTable("Cart");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__UserId__6754599E");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CartItem__3214EC075C63E1BC");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.CartItemIdNavigation)
                .HasForeignKey<CartItem>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__Id__68487DD7");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItemProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__Produ__66603565");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC07E2DF8229");

            entity.ToTable("Category");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Name).HasColumnType("text");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK__Category__Parent__6D0D32F4");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Device__3214EC07D5BF757C");

            entity.ToTable("Device");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Price).HasColumnType("decimal(8, 0)");
            entity.Property(e => e.Serial)
                .HasMaxLength(24)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.WarrantyExpiryDate).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Devices)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Device__UserId__6B24EA82");
        });

        modelBuilder.Entity<NutritionReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nutritio__3214EC073391F460");

            entity.ToTable("NutritionReport");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Device).WithMany(p => p.NutritionReports)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nutrition__Devic__693CA210");
        });

        modelBuilder.Entity<NutritionReportDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nutritio__3214EC07A8645C3E");

            entity.ToTable("NutritionReportDetail");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.RecordValue).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.Nutrition).WithMany(p => p.NutritionReportDetails)
                .HasForeignKey(d => d.NutritionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nutrition__Nutri__6A30C649");

            entity.HasOne(d => d.TargetValue).WithMany(p => p.NutritionReportDetails)
                .HasForeignKey(d => d.TargetValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nutrition__Targe__656C112C");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Order__3214EC07C8E4096B");

            entity.ToTable("Order");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 0)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__UserId__6EF57B66");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderDet__3214EC070C55819D");

            entity.ToTable("OrderDetail");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 0)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Device).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("FK__OrderDeta__Devic__73BA3083");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Order__6FE99F9F");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__OrderDeta__Produ__72C60C4A");
        });

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OTP__3214EC071DA27581");

            entity.ToTable("OTP");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.ExpiredDate).HasColumnType("datetime");
            entity.Property(e => e.IsUsed).HasColumnName("isUsed");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Otps)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OTP__UserId__5DCAEF64");
        });

        modelBuilder.Entity<Plant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Plant__3214EC07FCF21FCB");

            entity.ToTable("Plant");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3214EC07EABA0F59");

            entity.ToTable("Product");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 0)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Product__Categor__71D1E811");
        });

        modelBuilder.Entity<TargetOfPlant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TargetOf__3214EC07803A9494");

            entity.ToTable("TargetOfPlant");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Plant).WithMany(p => p.TargetOfPlants)
                .HasForeignKey(d => d.PlantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TargetOfP__Plant__6383C8BA");

            entity.HasOne(d => d.TargetValue).WithMany(p => p.TargetOfPlants)
                .HasForeignKey(d => d.TargetValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TargetOfP__Targe__6477ECF3");
        });

        modelBuilder.Entity<TargetValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TargetVa__3214EC077B49F9C7");

            entity.ToTable("TargetValue");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.MaxValue).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.MinValue).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ticket__3214EC0722C6D6A1");

            entity.ToTable("Ticket");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(8000)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Teachnician).WithMany(p => p.TicketTeachnicians)
                .HasForeignKey(d => d.TeachnicianId)
                .HasConstraintName("FK__Ticket__Teachnic__5FB337D6");

            entity.HasOne(d => d.User).WithMany(p => p.TicketUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ticket__UserId__6E01572D");
        });

        modelBuilder.Entity<TicketAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketAt__3214EC07EFE7C736");

            entity.ToTable("TicketAttachment");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketAttachments)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketAtt__Ticke__628FA481");
        });

        modelBuilder.Entity<TicketResponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketRe__3214EC0740578050");

            entity.ToTable("TicketResponse");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Message)
                .HasMaxLength(1000)
                .IsUnicode(false);

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketResponses)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketRes__Ticke__619B8048");
        });

        modelBuilder.Entity<TicketResponseAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketRe__3214EC073702D73E");

            entity.ToTable("TicketResponseAttachment");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);

            entity.HasOne(d => d.TicketResponse).WithMany(p => p.TicketResponseAttachments)
                .HasForeignKey(d => d.TicketResponseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketRes__Ticke__60A75C0F");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC0732FB8429");

            entity.ToTable("Transaction");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.FinishedTransactionAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentLinkId)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TransactionReference)
                .HasMaxLength(300)
                .IsUnicode(false);

            entity.HasOne(d => d.Order).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__Order__70DDC3D8");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC070FB7DAAB");

            entity.ToTable("User");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(800)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserAddr__3214EC072843B982");

            entity.ToTable("UserAddress");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address)
                .HasMaxLength(800)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Name)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.UserAddresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserAddre__UserI__5EBF139D");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserToke__3214EC0711B4B965");

            entity.ToTable("UserToken");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AccesToken)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.UserTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserToken__UserI__6C190EBB");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

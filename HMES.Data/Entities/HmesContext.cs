using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Entities;

public partial class HmesContext : DbContext
{
    public HmesContext(DbContextOptions<HmesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CropHydroponicDevice> CropHydroponicDevices { get; set; }

    public virtual DbSet<NutritionReport> NutritionReports { get; set; }

    public virtual DbSet<NutritionReportDetail> NutritionReportDetails { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<RecNutrition> RecNutritions { get; set; }

    public virtual DbSet<RecNutritionDetail> RecNutritionDetails { get; set; }

    public virtual DbSet<TicketReport> TicketReports { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cart__3214EC0769B342A3");

            entity.ToTable("Cart");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__UserId__571DF1D5");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CartItem__3214EC073DFFB87C");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__CartI__5629CD9C");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.CartItem)
                .HasForeignKey<CartItem>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__Id__5812160E");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC0700AAA76B");

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

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Categories)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Category__Create__5441852A");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK__Category__Parent__5CD6CB2B");
        });

        modelBuilder.Entity<CropHydroponicDevice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CropHydr__3214EC07EE6F0EB0");

            entity.ToTable("CropHydroponicDevice");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.CropHydroponicDevices)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CropHydro__UserI__5AEE82B9");
        });

        modelBuilder.Entity<NutritionReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nutritio__3214EC07D29A254A");

            entity.ToTable("NutritionReport");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Device).WithMany(p => p.NutritionReports)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nutrition__Devic__59063A47");
        });

        modelBuilder.Entity<NutritionReportDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nutritio__3214EC07C865E0D8");

            entity.ToTable("NutritionReportDetail");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Record).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Nutrition).WithMany(p => p.NutritionReportDetails)
                .HasForeignKey(d => d.NutritionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nutrition__Nutri__59FA5E80");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Order__3214EC07BDBD9E4F");

            entity.ToTable("Order");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__UserId__5EBF139D");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderDet__3214EC07BEAA3F7A");

            entity.ToTable("OrderDetail");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Order__5FB337D6");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Produ__628FA481");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3214EC07601502C5");

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
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Product__Categor__619B8048");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Product__Created__5535A963");
        });

        modelBuilder.Entity<RecNutrition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RecNutri__3214EC0708CEF2FB");

            entity.ToTable("RecNutrition");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RecNutritions)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RecNutrit__Creat__52593CB8");
        });

        modelBuilder.Entity<RecNutritionDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RecNutri__3214EC07C3110AF5");

            entity.ToTable("RecNutritionDetail");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Record).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.RecNutrition).WithMany(p => p.RecNutritionDetails)
                .HasForeignKey(d => d.RecNutritionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RecNutrit__RecNu__534D60F1");
        });

        modelBuilder.Entity<TicketReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketRe__3214EC073B5FF7AE");

            entity.ToTable("TicketReport");

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

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.TicketReport)
                .HasForeignKey<TicketReport>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketReport__Id__5DCAEF64");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC0737004287");

            entity.ToTable("Transaction");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__Order__60A75C0F");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07B9CB862C");

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
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserToke__3214EC075B395BA1");

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
                .HasConstraintName("FK__UserToken__UserI__5BE2A6F2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

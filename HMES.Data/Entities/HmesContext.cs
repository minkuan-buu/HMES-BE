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

    public HmesContext()
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceItem> DeviceItems { get; set; }

    public virtual DbSet<NutritionReport> NutritionReports { get; set; }

    public virtual DbSet<NutritionReportDetail> NutritionReportDetails { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<Plant> Plants { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAttachment> ProductAttachments { get; set; }

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
            entity.HasKey(e => e.Id).HasName("PK__Cart__3214EC076FF1E8EB");

            entity.ToTable("Cart");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__UserId__6B24EA82");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CartItem__3214EC0788AADC23");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__CartI__778AC167");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__Produ__6A30C649");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC078E3D49D5");

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
                .HasConstraintName("FK__Category__Parent__6FE99F9F");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Device__3214EC07CD85BD6C");

            entity.ToTable("Device");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 0)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DeviceItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DeviceIt__3214EC07B740241F");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.LastSeen).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.RefreshCycleHours).HasDefaultValue(5);
            entity.Property(e => e.Serial)
                .HasMaxLength(24)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.WarrantyExpiryDate).HasColumnType("datetime");

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceItems)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DeviceIte__Devic__787EE5A0");

            entity.HasOne(d => d.Plant).WithMany(p => p.DeviceItems)
                .HasForeignKey(d => d.PlantId)
                .HasConstraintName("FK__DeviceIte__Plant__7A672E12");

            entity.HasOne(d => d.User).WithMany(p => p.DeviceItems)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__DeviceIte__UserI__6E01572D");
        });

        modelBuilder.Entity<NutritionReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nutritio__3214EC070FD9AC57");

            entity.ToTable("NutritionReport");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.DeviceItem).WithMany(p => p.NutritionReports)
                .HasForeignKey(d => d.DeviceItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nutrition__Devic__6C190EBB");
        });

        modelBuilder.Entity<NutritionReportDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nutritio__3214EC07D655C376");

            entity.ToTable("NutritionReportDetail");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.RecordValue).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.Nutrition).WithMany(p => p.NutritionReportDetails)
                .HasForeignKey(d => d.NutritionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nutrition__Nutri__6D0D32F4");

            entity.HasOne(d => d.TargetValue).WithMany(p => p.NutritionReportDetails)
                .HasForeignKey(d => d.TargetValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nutrition__Targe__693CA210");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Order__3214EC07A19519F8");

            entity.ToTable("Order");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 0)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.UserAddress).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserAddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__UserAddre__7B5B524B");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__UserId__71D1E811");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderDet__3214EC07753CC1E6");

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
                .HasConstraintName("FK__OrderDeta__Devic__76969D2E");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Order__72C60C4A");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__OrderDeta__Produ__75A278F5");
        });

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OTP__3214EC07DC371D95");

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
                .HasConstraintName("FK__OTP__UserId__619B8048");
        });

        modelBuilder.Entity<Plant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Plant__3214EC073A31EC4B");

            entity.ToTable("Plant");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3214EC07E0CFB150");

            entity.ToTable("Product");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.MainImage)
                .HasMaxLength(2000)
                .IsUnicode(false);
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
                .HasConstraintName("FK__Product__Categor__74AE54BC");
        });

        modelBuilder.Entity<ProductAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductA__3214EC07EC5BD512");

            entity.ToTable("ProductAttachment");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAttachments)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductAt__Produ__7C4F7684");
        });

        modelBuilder.Entity<TargetOfPlant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TargetOf__3214EC070453D9FD");

            entity.ToTable("TargetOfPlant");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Plant).WithMany(p => p.TargetOfPlants)
                .HasForeignKey(d => d.PlantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TargetOfP__Plant__6754599E");

            entity.HasOne(d => d.TargetValue).WithMany(p => p.TargetOfPlants)
                .HasForeignKey(d => d.TargetValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TargetOfP__Targe__68487DD7");
        });

        modelBuilder.Entity<TargetValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TargetVa__3214EC073DD92AEA");

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
            entity.HasKey(e => e.Id).HasName("PK__Ticket__3214EC0717D2F02F");

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

            entity.HasOne(d => d.DeviceItem).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.DeviceItemId)
                .HasConstraintName("FK__Ticket__DeviceIt__797309D9");

            entity.HasOne(d => d.Technician).WithMany(p => p.TicketTechnicians)
                .HasForeignKey(d => d.TechnicianId)
                .HasConstraintName("FK__Ticket__Technici__6383C8BA");

            entity.HasOne(d => d.TransferToNavigation).WithMany(p => p.TicketTransferToNavigations)
                .HasForeignKey(d => d.TransferTo)
                .HasConstraintName("FK__Ticket__Transfer__7E37BEF6");

            entity.HasOne(d => d.User).WithMany(p => p.TicketUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ticket__UserId__70DDC3D8");
        });

        modelBuilder.Entity<TicketAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketAt__3214EC074667E8CF");

            entity.ToTable("TicketAttachment");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketAttachments)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketAtt__Ticke__66603565");
        });

        modelBuilder.Entity<TicketResponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketRe__3214EC07BE834E90");

            entity.ToTable("TicketResponse");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Message)
                .HasMaxLength(1000)
                .IsUnicode(false);

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketResponses)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketRes__Ticke__656C112C");

            entity.HasOne(d => d.User).WithMany(p => p.TicketResponses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketRes__UserI__7D439ABD");
        });

        modelBuilder.Entity<TicketResponseAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketRe__3214EC07AD4E33BE");

            entity.ToTable("TicketResponseAttachment");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);

            entity.HasOne(d => d.TicketResponse).WithMany(p => p.TicketResponseAttachments)
                .HasForeignKey(d => d.TicketResponseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketRes__Ticke__6477ECF3");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC07F2CEAE24");

            entity.ToTable("Transaction");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.FinishedTransactionAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentLinkId)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(20)
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
                .HasConstraintName("FK__Transacti__Order__73BA3083");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC0788BE3343");

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
            entity.HasKey(e => e.Id).HasName("PK__UserAddr__3214EC07E48CC3F4");

            entity.ToTable("UserAddress");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address)
                .HasMaxLength(800)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.District)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Name)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.Province)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Ward)
                .HasMaxLength(300)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.UserAddresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserAddre__UserI__628FA481");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserToke__3214EC071BD27455");

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
                .HasConstraintName("FK__UserToken__UserI__6EF57B66");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

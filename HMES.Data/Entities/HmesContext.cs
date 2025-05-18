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

    public virtual DbSet<GrowthPhase> GrowthPhases { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NutritionReport> NutritionReports { get; set; }

    public virtual DbSet<NutritionReportDetail> NutritionReportDetails { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<Plant> Plants { get; set; }

    public virtual DbSet<PlantOfPhase> PlantOfPhases { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAttachment> ProductAttachments { get; set; }

    public virtual DbSet<TargetOfPhase> TargetOfPhases { get; set; }

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
            entity.HasKey(e => e.Id).HasName("PK__Cart__3214EC07782C90F7");

            entity.ToTable("Cart");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__UserId__01142BA1");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CartItem__3214EC0783EE3047");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__CartI__02FC7413");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__Produ__02084FDA");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC078835A04C");

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
                .HasConstraintName("FK__Category__Parent__70DDC3D8");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Device__3214EC071FB33029");

            entity.ToTable("Device");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(3000)
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
            entity.HasKey(e => e.Id).HasName("PK__DeviceIt__3214EC0780B58F15");

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
                .HasConstraintName("FK__DeviceIte__Devic__7A672E12");

            entity.HasOne(d => d.Order).WithMany(p => p.DeviceItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DeviceIte__Order__7D439ABD");

            entity.HasOne(d => d.Phase).WithMany(p => p.DeviceItems)
                .HasForeignKey(d => d.PhaseId)
                .HasConstraintName("FK__DeviceIte__Phase__17F790F9");

            entity.HasOne(d => d.Plant).WithMany(p => p.DeviceItems)
                .HasForeignKey(d => d.PlantId)
                .HasConstraintName("FK__DeviceIte__Plant__7C4F7684");

            entity.HasOne(d => d.User).WithMany(p => p.DeviceItems)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__DeviceIte__UserI__7B5B524B");
        });

        modelBuilder.Entity<GrowthPhase>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GrowthPh__3214EC070E882127");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.GrowthPhases)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__GrowthPha__UserI__07C12930");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC07AAD9A1BC");

            entity.ToTable("Notification");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.NotificationType).HasMaxLength(50);
            entity.Property(e => e.ReadAt).HasColumnType("datetime");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Sender).WithMany(p => p.NotificationSenders)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK__Notificat__Sende__06CD04F7");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__05D8E0BE");
        });

        modelBuilder.Entity<NutritionReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nutritio__3214EC075FDEB022");

            entity.ToTable("NutritionReport");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.DeviceItem).WithMany(p => p.NutritionReports)
                .HasForeignKey(d => d.DeviceItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nutrition__Devic__7E37BEF6");
        });

        modelBuilder.Entity<NutritionReportDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nutritio__3214EC074A107B83");

            entity.ToTable("NutritionReportDetail");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.RecordValue).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.Nutrition).WithMany(p => p.NutritionReportDetails)
                .HasForeignKey(d => d.NutritionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nutrition__Nutri__00200768");

            entity.HasOne(d => d.TargetValue).WithMany(p => p.NutritionReportDetails)
                .HasForeignKey(d => d.TargetValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nutrition__Targe__7F2BE32F");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Order__3214EC07EEFE7A5C");

            entity.ToTable("Order");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ShippingFee).HasColumnType("decimal(10, 0)");
            entity.Property(e => e.ShippingOrderCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 0)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.UserAddress).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserAddressId)
                .HasConstraintName("FK__Order__UserAddre__6C190EBB");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__UserId__6B24EA82");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderDet__3214EC07E1B25937");

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
                .HasConstraintName("FK__OrderDeta__Devic__6EF57B66");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Order__6D0D32F4");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__OrderDeta__Produ__6E01572D");
        });

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OTP__3214EC07CC8920A0");

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
                .HasConstraintName("FK__OTP__UserId__03F0984C");
        });

        modelBuilder.Entity<Plant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Plant__3214EC071A4E3C78");

            entity.ToTable("Plant");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PlantOfPhase>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlantOfP__3214EC07C5E91807");

            entity.ToTable("PlantOfPhase");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Phase).WithMany(p => p.PlantOfPhases)
                .HasForeignKey(d => d.PhaseId)
                .HasConstraintName("FK__PlantOfPh__Phase__09A971A2");

            entity.HasOne(d => d.Plant).WithMany(p => p.PlantOfPhases)
                .HasForeignKey(d => d.PlantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PlantOfPh__Plant__08B54D69");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3214EC07BA091B18");

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
                .HasConstraintName("FK__Product__Categor__6A30C649");
        });

        modelBuilder.Entity<ProductAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductA__3214EC072F2A74E5");

            entity.ToTable("ProductAttachment");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAttachments)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductAt__Produ__04E4BC85");
        });

        modelBuilder.Entity<TargetOfPhase>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TargetOf__3214EC07CFDB29B5");

            entity.ToTable("TargetOfPhase");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.PlantOfPhase).WithMany(p => p.TargetOfPhases)
                .HasForeignKey(d => d.PlantOfPhaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TargetOfP__Plant__0B91BA14");

            entity.HasOne(d => d.TargetValue).WithMany(p => p.TargetOfPhases)
                .HasForeignKey(d => d.TargetValueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TargetOfP__Targe__0A9D95DB");
        });

        modelBuilder.Entity<TargetValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TargetVa__3214EC0798D0DA8E");

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
            entity.HasKey(e => e.Id).HasName("PK__Ticket__3214EC076C6897A7");

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
                .HasConstraintName("FK__Ticket__DeviceIt__72C60C4A");

            entity.HasOne(d => d.Technician).WithMany(p => p.TicketTechnicians)
                .HasForeignKey(d => d.TechnicianId)
                .HasConstraintName("FK__Ticket__Technici__73BA3083");

            entity.HasOne(d => d.TransferToNavigation).WithMany(p => p.TicketTransferToNavigations)
                .HasForeignKey(d => d.TransferTo)
                .HasConstraintName("FK__Ticket__Transfer__74AE54BC");

            entity.HasOne(d => d.User).WithMany(p => p.TicketUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ticket__UserId__71D1E811");
        });

        modelBuilder.Entity<TicketAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketAt__3214EC0759C15B65");

            entity.ToTable("TicketAttachment");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketAttachments)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketAtt__Ticke__75A278F5");
        });

        modelBuilder.Entity<TicketResponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketRe__3214EC07F398E87F");

            entity.ToTable("TicketResponse");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Message)
                .HasMaxLength(1000)
                .IsUnicode(false);

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketResponses)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketRes__Ticke__76969D2E");

            entity.HasOne(d => d.User).WithMany(p => p.TicketResponses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketRes__UserI__778AC167");
        });

        modelBuilder.Entity<TicketResponseAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketRe__3214EC0788916A1A");

            entity.ToTable("TicketResponseAttachment");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Attachment)
                .HasMaxLength(2000)
                .IsUnicode(false);

            entity.HasOne(d => d.TicketResponse).WithMany(p => p.TicketResponseAttachments)
                .HasForeignKey(d => d.TicketResponseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketRes__Ticke__787EE5A0");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC0764F34F43");

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
                .HasConstraintName("FK__Transacti__Order__6FE99F9F");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07BF4231EA");

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
            entity.HasKey(e => e.Id).HasName("PK__UserAddr__3214EC07A95D02E8");

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
                .HasConstraintName("FK__UserAddre__UserI__693CA210");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserToke__3214EC0791CB096E");

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
                .HasConstraintName("FK__UserToken__UserI__797309D9");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

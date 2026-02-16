using AiModoo.Core.Entities.POS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.POS;

public class PosConfigConfiguration : IEntityTypeConfiguration<PosConfig>
{
    public void Configure(EntityTypeBuilder<PosConfig> builder)
    {
        builder.ToTable("PosConfigs");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.NameAr).HasMaxLength(200);
        builder.Property(c => c.OpeningBalance).HasColumnType("decimal(18,2)");

        builder.HasOne(c => c.Warehouse).WithMany().HasForeignKey(c => c.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.StockLocation).WithMany().HasForeignKey(c => c.StockLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Journal).WithMany().HasForeignKey(c => c.JournalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Pricelist).WithMany().HasForeignKey(c => c.PricelistId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.DefaultTax).WithMany().HasForeignKey(c => c.DefaultTaxId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PosSessionConfiguration : IEntityTypeConfiguration<PosSession>
{
    public void Configure(EntityTypeBuilder<PosSession> builder)
    {
        builder.ToTable("PosSessions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.UserId).HasMaxLength(450);
        builder.Property(s => s.UserName).HasMaxLength(200);
        builder.Property(s => s.OpeningBalance).HasColumnType("decimal(18,2)");
        builder.Property(s => s.ClosingBalance).HasColumnType("decimal(18,2)");
        builder.Property(s => s.TotalSales).HasColumnType("decimal(18,2)");
        builder.Property(s => s.TotalReturns).HasColumnType("decimal(18,2)");
        builder.Property(s => s.TotalTax).HasColumnType("decimal(18,2)");
        builder.Property(s => s.TotalDiscount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.Notes).HasMaxLength(1000);

        builder.HasOne(s => s.Config).WithMany(c => c.Sessions).HasForeignKey(s => s.ConfigId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.JournalEntry).WithMany().HasForeignKey(s => s.JournalEntryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PosOrderConfiguration : IEntityTypeConfiguration<PosOrder>
{
    public void Configure(EntityTypeBuilder<PosOrder> builder)
    {
        builder.ToTable("PosOrders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Number).HasMaxLength(50).IsRequired();
        builder.HasIndex(o => o.Number).IsUnique();
        builder.Property(o => o.PartnerName).HasMaxLength(200);
        builder.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(o => o.TaxTotal).HasColumnType("decimal(18,2)");
        builder.Property(o => o.DiscountTotal).HasColumnType("decimal(18,2)");
        builder.Property(o => o.Total).HasColumnType("decimal(18,2)");
        builder.Property(o => o.AmountPaid).HasColumnType("decimal(18,2)");
        builder.Property(o => o.Change).HasColumnType("decimal(18,2)");
        builder.Property(o => o.Notes).HasMaxLength(500);

        builder.HasOne(o => o.Session).WithMany(s => s.Orders).HasForeignKey(o => o.SessionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Partner).WithMany().HasForeignKey(o => o.PartnerId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PosOrderLineConfiguration : IEntityTypeConfiguration<PosOrderLine>
{
    public void Configure(EntityTypeBuilder<PosOrderLine> builder)
    {
        builder.ToTable("PosOrderLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.ProductName).HasMaxLength(200);
        builder.Property(l => l.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,4)");
        builder.Property(l => l.Discount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(l => l.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Total).HasColumnType("decimal(18,2)");

        builder.HasOne(l => l.Order).WithMany(o => o.Lines).HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PosPaymentConfiguration : IEntityTypeConfiguration<PosPayment>
{
    public void Configure(EntityTypeBuilder<PosPayment> builder)
    {
        builder.ToTable("PosPayments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");

        builder.HasOne(p => p.Order).WithMany(o => o.Payments).HasForeignKey(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.PaymentMethod).WithMany().HasForeignKey(p => p.PaymentMethodId).OnDelete(DeleteBehavior.Restrict);
    }
}

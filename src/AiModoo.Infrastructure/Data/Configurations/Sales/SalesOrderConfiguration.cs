using AiModoo.Core.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Sales;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Number).HasMaxLength(50).IsRequired();
        builder.HasIndex(o => o.Number).IsUnique();
        builder.Property(o => o.ClientOrderRef).HasMaxLength(200);
        builder.Property(o => o.Notes).HasMaxLength(1000);
        builder.Property(o => o.NotesAr).HasMaxLength(1000);
        builder.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(o => o.TaxTotal).HasColumnType("decimal(18,2)");
        builder.Property(o => o.Total).HasColumnType("decimal(18,2)");

        builder.HasOne(o => o.Partner).WithMany(p => p.SalesOrders).HasForeignKey(o => o.PartnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Pricelist).WithMany().HasForeignKey(o => o.PricelistId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Currency).WithMany().HasForeignKey(o => o.CurrencyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.PaymentTerm).WithMany().HasForeignKey(o => o.PaymentTermId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Invoice).WithMany().HasForeignKey(o => o.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.DeliveryPicking).WithMany().HasForeignKey(o => o.DeliveryPickingId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.Status);
    }
}

public class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.ToTable("SalesOrderLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Description).HasMaxLength(500).IsRequired();
        builder.Property(l => l.DescriptionAr).HasMaxLength(500);
        builder.Property(l => l.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(l => l.DeliveredQty).HasColumnType("decimal(18,4)");
        builder.Property(l => l.InvoicedQty).HasColumnType("decimal(18,4)");
        builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Discount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(l => l.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Total).HasColumnType("decimal(18,2)");

        builder.HasOne(l => l.Order).WithMany(o => o.Lines).HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.Uom).WithMany().HasForeignKey(l => l.UomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.Tax).WithMany().HasForeignKey(l => l.TaxId).OnDelete(DeleteBehavior.Restrict);
    }
}

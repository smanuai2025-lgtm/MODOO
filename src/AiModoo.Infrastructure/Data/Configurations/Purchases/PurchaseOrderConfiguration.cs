using AiModoo.Core.Entities.Purchases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Purchases;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Number).HasMaxLength(50).IsRequired();
        builder.HasIndex(o => o.Number).IsUnique();
        builder.Property(o => o.VendorReference).HasMaxLength(200);
        builder.Property(o => o.Notes).HasMaxLength(1000);
        builder.Property(o => o.NotesAr).HasMaxLength(1000);
        builder.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(o => o.TaxTotal).HasColumnType("decimal(18,2)");
        builder.Property(o => o.Total).HasColumnType("decimal(18,2)");

        builder.HasOne(o => o.Vendor).WithMany(p => p.PurchaseOrders).HasForeignKey(o => o.VendorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.PaymentTerm).WithMany().HasForeignKey(o => o.PaymentTermId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Currency).WithMany().HasForeignKey(o => o.CurrencyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Bill).WithMany().HasForeignKey(o => o.BillId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.ReceiptPicking).WithMany().HasForeignKey(o => o.ReceiptPickingId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.Status);
    }
}

public class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToTable("PurchaseOrderLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Description).HasMaxLength(500).IsRequired();
        builder.Property(l => l.DescriptionAr).HasMaxLength(500);
        builder.Property(l => l.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(l => l.ReceivedQty).HasColumnType("decimal(18,4)");
        builder.Property(l => l.BilledQty).HasColumnType("decimal(18,4)");
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

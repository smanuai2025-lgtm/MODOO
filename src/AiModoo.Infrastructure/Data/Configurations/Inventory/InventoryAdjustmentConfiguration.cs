using AiModoo.Core.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Inventory;

public class InventoryAdjustmentConfiguration : IEntityTypeConfiguration<InventoryAdjustment>
{
    public void Configure(EntityTypeBuilder<InventoryAdjustment> builder)
    {
        builder.ToTable("InventoryAdjustments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Notes).HasMaxLength(2000);

        builder.HasOne(a => a.Warehouse).WithMany()
            .HasForeignKey(a => a.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Location).WithMany()
            .HasForeignKey(a => a.LocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class InventoryAdjustmentLineConfiguration : IEntityTypeConfiguration<InventoryAdjustmentLine>
{
    public void Configure(EntityTypeBuilder<InventoryAdjustmentLine> builder)
    {
        builder.ToTable("InventoryAdjustmentLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.TheoreticalQty).HasColumnType("decimal(18,4)");
        builder.Property(l => l.RealQty).HasColumnType("decimal(18,4)");
        builder.Ignore(l => l.Difference);

        builder.HasOne(l => l.Adjustment).WithMany(a => a.Lines)
            .HasForeignKey(l => l.AdjustmentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Product).WithMany()
            .HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.Location).WithMany()
            .HasForeignKey(l => l.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.Lot).WithMany()
            .HasForeignKey(l => l.LotId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReorderingRuleConfiguration : IEntityTypeConfiguration<ReorderingRule>
{
    public void Configure(EntityTypeBuilder<ReorderingRule> builder)
    {
        builder.ToTable("ReorderingRules");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.MinQty).HasColumnType("decimal(18,4)");
        builder.Property(r => r.MaxQty).HasColumnType("decimal(18,4)");
        builder.Property(r => r.OrderQty).HasColumnType("decimal(18,4)");

        builder.HasOne(r => r.Product).WithMany(p => p.ReorderingRules)
            .HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Warehouse).WithMany(w => w.ReorderingRules)
            .HasForeignKey(r => r.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Location).WithMany()
            .HasForeignKey(r => r.LocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

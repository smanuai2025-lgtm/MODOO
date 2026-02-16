using AiModoo.Core.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Inventory;

public class StockPickingTypeConfiguration : IEntityTypeConfiguration<StockPickingType>
{
    public void Configure(EntityTypeBuilder<StockPickingType> builder)
    {
        builder.ToTable("StockPickingTypes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.NameAr).HasMaxLength(200);
        builder.Property(t => t.Sequence).HasMaxLength(50);

        builder.HasOne(t => t.Warehouse).WithMany(w => w.PickingTypes)
            .HasForeignKey(t => t.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.DefaultSourceLocation).WithMany()
            .HasForeignKey(t => t.DefaultSourceLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.DefaultDestLocation).WithMany()
            .HasForeignKey(t => t.DefaultDestLocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockPickingConfiguration : IEntityTypeConfiguration<StockPicking>
{
    public void Configure(EntityTypeBuilder<StockPicking> builder)
    {
        builder.ToTable("StockPickings");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.PartnerName).HasMaxLength(200);
        builder.Property(p => p.SourceDocument).HasMaxLength(200);
        builder.Property(p => p.Notes).HasMaxLength(2000);

        builder.HasIndex(p => p.Name);

        builder.HasOne(p => p.PickingType).WithMany(t => t.Pickings)
            .HasForeignKey(p => p.PickingTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.SourceLocation).WithMany()
            .HasForeignKey(p => p.SourceLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.DestLocation).WithMany()
            .HasForeignKey(p => p.DestLocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

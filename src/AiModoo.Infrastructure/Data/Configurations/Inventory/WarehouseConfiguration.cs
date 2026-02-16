using AiModoo.Core.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Inventory;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).HasMaxLength(200).IsRequired();
        builder.Property(w => w.NameAr).HasMaxLength(200);
        builder.Property(w => w.Code).HasMaxLength(20).IsRequired();
        builder.Property(w => w.Address).HasMaxLength(500);

        builder.HasIndex(w => w.Code).IsUnique();

        builder.HasOne(w => w.StockLocation).WithMany()
            .HasForeignKey(w => w.StockLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(w => w.InputLocation).WithMany()
            .HasForeignKey(w => w.InputLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(w => w.OutputLocation).WithMany()
            .HasForeignKey(w => w.OutputLocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Name).HasMaxLength(200).IsRequired();
        builder.Property(l => l.NameAr).HasMaxLength(200);
        builder.Property(l => l.CompleteName).HasMaxLength(500);

        builder.HasOne(l => l.Warehouse).WithMany(w => w.Locations)
            .HasForeignKey(l => l.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.ParentLocation).WithMany(l => l.ChildLocations)
            .HasForeignKey(l => l.ParentLocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

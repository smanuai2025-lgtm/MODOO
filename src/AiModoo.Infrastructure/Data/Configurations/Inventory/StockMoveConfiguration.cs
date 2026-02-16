using AiModoo.Core.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Inventory;

public class StockMoveConfiguration : IEntityTypeConfiguration<StockMove>
{
    public void Configure(EntityTypeBuilder<StockMove> builder)
    {
        builder.ToTable("StockMoves");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Reference).HasMaxLength(200);
        builder.Property(m => m.ProductUomQty).HasColumnType("decimal(18,4)");
        builder.Property(m => m.UnitCost).HasColumnType("decimal(18,2)");
        builder.Property(m => m.TotalCost).HasColumnType("decimal(18,2)");

        builder.HasOne(m => m.Product).WithMany(p => p.StockMoves)
            .HasForeignKey(m => m.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Uom).WithMany()
            .HasForeignKey(m => m.UomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Picking).WithMany(p => p.Moves)
            .HasForeignKey(m => m.PickingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.SourceLocation).WithMany()
            .HasForeignKey(m => m.SourceLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.DestLocation).WithMany()
            .HasForeignKey(m => m.DestLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Lot).WithMany(l => l.StockMoves)
            .HasForeignKey(m => m.LotId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockQuantConfiguration : IEntityTypeConfiguration<StockQuant>
{
    public void Configure(EntityTypeBuilder<StockQuant> builder)
    {
        builder.ToTable("StockQuants");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(q => q.ReservedQuantity).HasColumnType("decimal(18,4)");
        builder.Ignore(q => q.AvailableQuantity);

        builder.HasIndex(q => new { q.ProductId, q.LocationId, q.LotId }).IsUnique()
            .HasFilter("[LotId] IS NOT NULL");

        builder.HasOne(q => q.Product).WithMany(p => p.StockQuants)
            .HasForeignKey(q => q.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(q => q.Location).WithMany(l => l.StockQuants)
            .HasForeignKey(q => q.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(q => q.Lot).WithMany(l => l.StockQuants)
            .HasForeignKey(q => q.LotId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockLotConfiguration : IEntityTypeConfiguration<StockLot>
{
    public void Configure(EntityTypeBuilder<StockLot> builder)
    {
        builder.ToTable("StockLots");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Name).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Notes).HasMaxLength(1000);

        builder.HasOne(l => l.Product).WithMany(p => p.StockLots)
            .HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

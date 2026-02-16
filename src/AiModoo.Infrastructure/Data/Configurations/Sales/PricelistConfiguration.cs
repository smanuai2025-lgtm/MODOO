using AiModoo.Core.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Sales;

public class PricelistConfiguration : IEntityTypeConfiguration<Pricelist>
{
    public void Configure(EntityTypeBuilder<Pricelist> builder)
    {
        builder.ToTable("Pricelists");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.NameAr).HasMaxLength(200);
        builder.HasOne(p => p.Currency).WithMany().HasForeignKey(p => p.CurrencyId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PricelistItemConfiguration : IEntityTypeConfiguration<PricelistItem>
{
    public void Configure(EntityTypeBuilder<PricelistItem> builder)
    {
        builder.ToTable("PricelistItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.FixedPrice).HasColumnType("decimal(18,2)");
        builder.Property(i => i.PercentDiscount).HasColumnType("decimal(5,2)");
        builder.HasOne(i => i.Pricelist).WithMany(p => p.Items).HasForeignKey(i => i.PricelistId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Category).WithMany().HasForeignKey(i => i.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

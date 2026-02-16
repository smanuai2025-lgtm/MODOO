using AiModoo.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Accounting;

public class TaxConfiguration : IEntityTypeConfiguration<Tax>
{
    public void Configure(EntityTypeBuilder<Tax> builder)
    {
        builder.ToTable("Taxes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.NameAr).HasMaxLength(200);
        builder.Property(t => t.Rate).HasColumnType("decimal(8,4)");
        builder.HasOne(t => t.SalesAccount).WithMany().HasForeignKey(t => t.SalesAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.PurchaseAccount).WithMany().HasForeignKey(t => t.PurchaseAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}

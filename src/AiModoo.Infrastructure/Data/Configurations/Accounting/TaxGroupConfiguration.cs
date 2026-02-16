using AiModoo.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Accounting;

public class TaxGroupConfiguration : IEntityTypeConfiguration<TaxGroup>
{
    public void Configure(EntityTypeBuilder<TaxGroup> builder)
    {
        builder.ToTable("TaxGroups");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
        builder.Property(g => g.NameAr).HasMaxLength(100);
        builder.Ignore(g => g.TotalRate);
    }
}

public class TaxGroupDetailConfiguration : IEntityTypeConfiguration<TaxGroupDetail>
{
    public void Configure(EntityTypeBuilder<TaxGroupDetail> builder)
    {
        builder.ToTable("TaxGroupDetails");
        builder.HasKey(d => d.Id);
        builder.HasOne(d => d.TaxGroup).WithMany(g => g.Details).HasForeignKey(d => d.TaxGroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.Tax).WithMany().HasForeignKey(d => d.TaxId).OnDelete(DeleteBehavior.Restrict);
    }
}

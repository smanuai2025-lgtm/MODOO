using AiModoo.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Accounting;

public class FiscalYearConfiguration : IEntityTypeConfiguration<FiscalYear>
{
    public void Configure(EntityTypeBuilder<FiscalYear> builder)
    {
        builder.ToTable("FiscalYears");
        builder.HasKey(y => y.Id);
        builder.Property(y => y.Name).HasMaxLength(100).IsRequired();
        builder.HasMany(y => y.Periods).WithOne(p => p.FiscalYear).HasForeignKey(p => p.FiscalYearId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class FiscalPeriodConfiguration : IEntityTypeConfiguration<FiscalPeriod>
{
    public void Configure(EntityTypeBuilder<FiscalPeriod> builder)
    {
        builder.ToTable("FiscalPeriods");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
    }
}

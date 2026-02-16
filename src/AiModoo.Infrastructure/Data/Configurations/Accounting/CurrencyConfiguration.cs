using AiModoo.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Accounting;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).HasMaxLength(10).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.NameAr).HasMaxLength(100);
        builder.Property(c => c.Symbol).HasMaxLength(10).IsRequired();
    }
}

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("ExchangeRates");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Rate).HasColumnType("decimal(18,6)");
        builder.HasOne(r => r.Currency).WithMany(c => c.ExchangeRates).HasForeignKey(r => r.CurrencyId).OnDelete(DeleteBehavior.Cascade);
    }
}

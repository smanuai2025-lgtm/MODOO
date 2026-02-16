using AiModoo.Core.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Sales;

public class PaymentTermConfiguration : IEntityTypeConfiguration<PaymentTerm>
{
    public void Configure(EntityTypeBuilder<PaymentTerm> builder)
    {
        builder.ToTable("PaymentTerms");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.NameAr).HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(500);
    }
}

public class PaymentTermLineConfiguration : IEntityTypeConfiguration<PaymentTermLine>
{
    public void Configure(EntityTypeBuilder<PaymentTermLine> builder)
    {
        builder.ToTable("PaymentTermLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Percentage).HasColumnType("decimal(5,2)");
        builder.HasOne(l => l.PaymentTerm).WithMany(p => p.Lines).HasForeignKey(l => l.PaymentTermId).OnDelete(DeleteBehavior.Cascade);
    }
}

using AiModoo.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Accounting;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Number).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.Number).IsUnique();
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.AllocatedAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Memo).HasMaxLength(500);
        builder.Property(p => p.PartnerName).HasMaxLength(200);
        builder.HasOne(p => p.Journal).WithMany().HasForeignKey(p => p.JournalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.JournalEntry).WithMany().HasForeignKey(p => p.JournalEntryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.PaymentMethod).WithMany().HasForeignKey(p => p.PaymentMethodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Currency).WithMany().HasForeignKey(p => p.CurrencyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Partner).WithMany().HasForeignKey(p => p.PartnerId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("PaymentMethods");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
        builder.Property(m => m.NameAr).HasMaxLength(100);
    }
}

using AiModoo.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Accounting;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Number).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.Number).IsUnique();
        builder.Property(e => e.Reference).HasMaxLength(200);
        builder.Property(e => e.Narration).HasMaxLength(1000);
        builder.Property(e => e.NarrationAr).HasMaxLength(1000);
        builder.Property(e => e.SourceDocument).HasMaxLength(100);
        builder.Property(e => e.SourceModule).HasMaxLength(50);
        builder.Property(e => e.TotalDebit).HasColumnType("decimal(18,2)");
        builder.Property(e => e.TotalCredit).HasColumnType("decimal(18,2)");
        builder.HasOne(e => e.Journal).WithMany().HasForeignKey(e => e.JournalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.FiscalPeriod).WithMany().HasForeignKey(e => e.FiscalPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ReversalOf).WithMany().HasForeignKey(e => e.ReversalOfId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(e => e.PostedById).HasMaxLength(450);
    }
}

public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Debit).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Credit).HasColumnType("decimal(18,2)");
        builder.Property(l => l.AmountCurrency).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Label).HasMaxLength(500);
        builder.Property(l => l.ReconcileGroupId).HasMaxLength(50);
        builder.HasOne(l => l.JournalEntry).WithMany(e => e.Lines).HasForeignKey(l => l.JournalEntryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Account).WithMany(a => a.JournalEntryLines).HasForeignKey(l => l.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.Currency).WithMany().HasForeignKey(l => l.CurrencyId).OnDelete(DeleteBehavior.Restrict);
    }
}

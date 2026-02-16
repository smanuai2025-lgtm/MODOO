using AiModoo.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Accounting;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("BankAccounts");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
        builder.Property(b => b.AccountNumber).HasMaxLength(50).IsRequired();
        builder.Property(b => b.BankName).HasMaxLength(200);
        builder.Property(b => b.Balance).HasColumnType("decimal(18,2)");
        builder.HasOne(b => b.Account).WithMany().HasForeignKey(b => b.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.Currency).WithMany().HasForeignKey(b => b.CurrencyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.Journal).WithMany().HasForeignKey(b => b.JournalId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class BankReconciliationConfiguration : IEntityTypeConfiguration<BankReconciliation>
{
    public void Configure(EntityTypeBuilder<BankReconciliation> builder)
    {
        builder.ToTable("BankReconciliations");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.StartingBalance).HasColumnType("decimal(18,2)");
        builder.Property(r => r.EndingBalance).HasColumnType("decimal(18,2)");
        builder.HasOne(r => r.BankAccount).WithMany().HasForeignKey(r => r.BankAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(r => r.Lines).WithOne(l => l.Reconciliation).HasForeignKey(l => l.ReconciliationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class BankReconciliationLineConfiguration : IEntityTypeConfiguration<BankReconciliationLine>
{
    public void Configure(EntityTypeBuilder<BankReconciliationLine> builder)
    {
        builder.ToTable("BankReconciliationLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Amount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Reference).HasMaxLength(200);
        builder.Property(l => l.Description).HasMaxLength(500);
        builder.Property(l => l.PartnerName).HasMaxLength(200);
        builder.HasOne(l => l.JournalEntryLine).WithMany().HasForeignKey(l => l.JournalEntryLineId).OnDelete(DeleteBehavior.Restrict);
    }
}

using AiModoo.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Accounting;

public class BankStatementConfiguration : IEntityTypeConfiguration<BankStatement>
{
    public void Configure(EntityTypeBuilder<BankStatement> builder)
    {
        builder.ToTable("BankStatements");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Reference).HasMaxLength(100);
        builder.Property(s => s.OpeningBalance).HasColumnType("decimal(18,2)");
        builder.Property(s => s.ClosingBalance).HasColumnType("decimal(18,2)");
        builder.HasOne(s => s.BankAccount).WithMany().HasForeignKey(s => s.BankAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class BankStatementLineConfiguration : IEntityTypeConfiguration<BankStatementLine>
{
    public void Configure(EntityTypeBuilder<BankStatementLine> builder)
    {
        builder.ToTable("BankStatementLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Reference).HasMaxLength(200);
        builder.Property(l => l.Description).HasMaxLength(500);
        builder.Property(l => l.PartnerName).HasMaxLength(200);
        builder.Property(l => l.Amount).HasColumnType("decimal(18,2)");
        builder.HasOne(l => l.Statement).WithMany(s => s.Lines).HasForeignKey(l => l.StatementId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.MatchedJournalEntryLine).WithMany().HasForeignKey(l => l.MatchedJournalEntryLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.Reconciliation).WithMany().HasForeignKey(l => l.ReconciliationId).OnDelete(DeleteBehavior.Restrict);
    }
}

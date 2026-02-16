using AiModoo.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Accounting;

public class JournalConfiguration : IEntityTypeConfiguration<Journal>
{
    public void Configure(EntityTypeBuilder<Journal> builder)
    {
        builder.ToTable("Journals");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Name).HasMaxLength(200).IsRequired();
        builder.Property(j => j.NameAr).HasMaxLength(200);
        builder.Property(j => j.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(j => j.Code).IsUnique();
        builder.Property(j => j.SequencePrefix).HasMaxLength(10);
        builder.HasOne(j => j.DefaultDebitAccount).WithMany().HasForeignKey(j => j.DefaultDebitAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(j => j.DefaultCreditAccount).WithMany().HasForeignKey(j => j.DefaultCreditAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}

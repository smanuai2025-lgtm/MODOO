using AiModoo.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Accounting;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(a => a.Code).IsUnique();
        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.NameAr).HasMaxLength(200);
        builder.HasOne(a => a.ParentAccount).WithMany(a => a.ChildAccounts).HasForeignKey(a => a.ParentAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Currency).WithMany().HasForeignKey(a => a.CurrencyId).OnDelete(DeleteBehavior.SetNull);
    }
}

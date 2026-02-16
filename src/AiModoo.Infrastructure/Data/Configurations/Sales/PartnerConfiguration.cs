using AiModoo.Core.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Sales;

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("Partners");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(300).IsRequired();
        builder.Property(p => p.NameAr).HasMaxLength(300);
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.Property(p => p.Phone).HasMaxLength(30);
        builder.Property(p => p.Mobile).HasMaxLength(30);
        builder.Property(p => p.Website).HasMaxLength(200);
        builder.Property(p => p.Street).HasMaxLength(300);
        builder.Property(p => p.City).HasMaxLength(100);
        builder.Property(p => p.State).HasMaxLength(100);
        builder.Property(p => p.Country).HasMaxLength(100);
        builder.Property(p => p.ZipCode).HasMaxLength(20);
        builder.Property(p => p.TaxNumber).HasMaxLength(50);
        builder.Property(p => p.CommercialRegister).HasMaxLength(50);
        builder.Property(p => p.CreditLimit).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Notes).HasMaxLength(2000);

        builder.HasOne(p => p.AccountReceivable).WithMany().HasForeignKey(p => p.AccountReceivableId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.AccountPayable).WithMany().HasForeignKey(p => p.AccountPayableId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.PaymentTerm).WithMany().HasForeignKey(p => p.PaymentTermId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Pricelist).WithMany().HasForeignKey(p => p.PricelistId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.PartnerType);
        builder.HasIndex(p => p.Email);
    }
}

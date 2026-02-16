using AiModoo.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Accounting;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Number).HasMaxLength(50).IsRequired();
        builder.HasIndex(i => i.Number).IsUnique();
        builder.Property(i => i.PartnerName).HasMaxLength(200);
        builder.Property(i => i.PartnerNameAr).HasMaxLength(200);
        builder.Property(i => i.Reference).HasMaxLength(200);
        builder.Property(i => i.Notes).HasMaxLength(1000);
        builder.Property(i => i.NotesAr).HasMaxLength(1000);
        builder.Property(i => i.SourceDocument).HasMaxLength(100);
        builder.Property(i => i.SourceModule).HasMaxLength(50);
        builder.Property(i => i.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TaxTotal).HasColumnType("decimal(18,2)");
        builder.Property(i => i.Total).HasColumnType("decimal(18,2)");
        builder.Property(i => i.AmountPaid).HasColumnType("decimal(18,2)");
        builder.Property(i => i.AmountDue).HasColumnType("decimal(18,2)");
        builder.HasOne(i => i.Journal).WithMany().HasForeignKey(i => i.JournalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.JournalEntry).WithMany().HasForeignKey(i => i.JournalEntryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.FiscalPeriod).WithMany().HasForeignKey(i => i.FiscalPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Currency).WithMany().HasForeignKey(i => i.CurrencyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.ReversalOf).WithMany().HasForeignKey(i => i.ReversalOfId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Partner).WithMany().HasForeignKey(i => i.PartnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.SalesOrder).WithMany().HasForeignKey(i => i.SalesOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.PurchaseOrder).WithMany().HasForeignKey(i => i.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Description).HasMaxLength(500).IsRequired();
        builder.Property(l => l.DescriptionAr).HasMaxLength(500);
        builder.Property(l => l.ProductName).HasMaxLength(200);
        builder.Property(l => l.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Discount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(l => l.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Total).HasColumnType("decimal(18,2)");
        builder.HasOne(l => l.Invoice).WithMany(i => i.Lines).HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Account).WithMany().HasForeignKey(l => l.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.Tax).WithMany().HasForeignKey(l => l.TaxId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable("PaymentAllocations");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Amount).HasColumnType("decimal(18,2)");
        builder.HasOne(a => a.Payment).WithMany(p => p.Allocations).HasForeignKey(a => a.PaymentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Invoice).WithMany(i => i.PaymentAllocations).HasForeignKey(a => a.InvoiceId).OnDelete(DeleteBehavior.Restrict);
    }
}

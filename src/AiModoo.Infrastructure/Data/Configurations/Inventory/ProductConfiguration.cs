using AiModoo.Core.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Inventory;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.NameAr).HasMaxLength(200);
        builder.HasOne(c => c.ParentCategory).WithMany(c => c.ChildCategories)
            .HasForeignKey(c => c.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class UnitOfMeasureCategoryConfiguration : IEntityTypeConfiguration<UnitOfMeasureCategory>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasureCategory> builder)
    {
        builder.ToTable("UnitOfMeasureCategories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.NameAr).HasMaxLength(100);
    }
}

public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("UnitOfMeasures");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).HasMaxLength(100).IsRequired();
        builder.Property(u => u.NameAr).HasMaxLength(100);
        builder.Property(u => u.Ratio).HasColumnType("decimal(18,6)");
        builder.Property(u => u.Rounding).HasColumnType("decimal(18,6)");
        builder.HasOne(u => u.Category).WithMany(c => c.UnitOfMeasures)
            .HasForeignKey(u => u.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(300).IsRequired();
        builder.Property(p => p.NameAr).HasMaxLength(300);
        builder.Property(p => p.InternalReference).HasMaxLength(100);
        builder.Property(p => p.Barcode).HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.DescriptionAr).HasMaxLength(2000);
        builder.Property(p => p.ImageUrl).HasMaxLength(500);
        builder.Property(p => p.SalePrice).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Cost).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Weight).HasColumnType("decimal(18,4)");
        builder.Property(p => p.Volume).HasColumnType("decimal(18,4)");

        builder.HasIndex(p => p.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");
        builder.HasIndex(p => p.InternalReference);

        builder.HasOne(p => p.Category).WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Uom).WithMany()
            .HasForeignKey(p => p.UomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.PurchaseUom).WithMany()
            .HasForeignKey(p => p.PurchaseUomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.AccountStockInput).WithMany()
            .HasForeignKey(p => p.AccountStockInputId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.AccountStockOutput).WithMany()
            .HasForeignKey(p => p.AccountStockOutputId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.AccountExpense).WithMany()
            .HasForeignKey(p => p.AccountExpenseId).OnDelete(DeleteBehavior.Restrict);
    }
}

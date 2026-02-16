using AiModoo.Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Identity;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.DescriptionAr).HasMaxLength(500);
        builder.Property(r => r.IsSystemRole).HasDefaultValue(false);
    }
}

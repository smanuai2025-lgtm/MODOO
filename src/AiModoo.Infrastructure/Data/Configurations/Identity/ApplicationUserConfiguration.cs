using AiModoo.Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Identity;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.FullNameAr).HasMaxLength(200);
        builder.Property(u => u.PreferredLanguage).HasMaxLength(10).HasDefaultValue("ar");
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.IsActive).HasDefaultValue(true);
    }
}

using AiModoo.Core.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiModoo.Infrastructure.Data.Configurations.Sales;

public class SalesTeamConfiguration : IEntityTypeConfiguration<SalesTeam>
{
    public void Configure(EntityTypeBuilder<SalesTeam> builder)
    {
        builder.ToTable("SalesTeams");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.NameAr).HasMaxLength(200);
        builder.Property(t => t.LeaderUserId).HasMaxLength(450);
    }
}

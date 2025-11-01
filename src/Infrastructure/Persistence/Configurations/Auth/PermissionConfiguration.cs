using EduCare.Domain.Entity.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Auth;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.HasKey(p => p.Id);

        entity.Property(p => p.Id).HasMaxLength(150).IsRequired();
        entity.Property(p => p.Feature).HasMaxLength(100).IsRequired();
        entity.Property(p => p.Action).HasMaxLength(100).IsRequired();
        entity.Property(p => p.Group).HasMaxLength(100).IsRequired();
        entity.Property(p => p.Description).HasMaxLength(300);
        entity.Property(p => p.IsBasic).IsRequired();

        entity.HasIndex(p => new { p.Feature, p.Action }).IsUnique();

        entity.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .HasPrincipalKey(p => p.Id);

        entity.ToTable("permission", SchemaNames.Auth);
    }
}
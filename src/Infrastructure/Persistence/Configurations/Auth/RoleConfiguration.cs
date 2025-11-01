using EduCare.Domain.Entity.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Auth;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.HasKey(r => r.Id);

        entity.Property(r => r.Name).IsRequired().HasMaxLength(100);

        entity.HasIndex(r => r.Name).IsUnique();

        entity.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .HasPrincipalKey(r => r.Id);

        entity.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .HasPrincipalKey(r => r.Id);

        entity.ToTable("role", SchemaNames.Auth);
    }
}
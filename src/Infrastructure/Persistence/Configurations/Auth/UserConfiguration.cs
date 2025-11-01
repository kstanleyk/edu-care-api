using EduCare.Domain.Entity.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Auth;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(u => u.Id);

        entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
        entity.Property(u => u.FullName).IsRequired().HasMaxLength(255);
        entity.Property(u => u.ProfileImageUrl).IsRequired().HasMaxLength(355);

        entity.HasIndex(u => u.Email).IsUnique();

        entity.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .HasPrincipalKey(u => u.Id);

        entity.ToTable("user", SchemaNames.Auth);
    }
}
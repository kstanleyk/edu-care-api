using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Core;

public class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.ToTable("Parents");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.OwnsOne(p => p.Name, name =>
        {
            name.Property(n => n.FirstName).HasMaxLength(100).IsRequired();
            name.Property(n => n.MiddleName).HasMaxLength(100).IsRequired(false);
            name.Property(n => n.LastName).HasMaxLength(100).IsRequired();
        });

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.OwnsOne(p => p.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200);
            address.Property(a => a.City).HasMaxLength(100);
            address.Property(a => a.State).HasMaxLength(100);
            address.Property(a => a.Country).HasMaxLength(100);
            address.Property(a => a.ZipCode).HasMaxLength(20);
        });

        builder.Property(p => p.Relationship)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.IsPrimaryContact)
            .IsRequired();

        builder.Property(p => p.CreatedOn)
            .IsRequired();

        builder.Property(p => p.ModifiedOn)
            .IsRequired(false);
    }
}
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Core;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.OwnsOne(o => o.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200);
            address.Property(a => a.City).HasMaxLength(100);
            address.Property(a => a.State).HasMaxLength(100);
            address.Property(a => a.Country).HasMaxLength(100);
            address.Property(a => a.ZipCode).HasMaxLength(20);
        });

        builder.HasMany(o => o.Schools)
            .WithOne()
            .HasForeignKey(s => s.OrganizationId);

        builder.Property(o => o.CreatedOn)
            .IsRequired();

        builder.Property(o => o.ModifiedOn)
            .IsRequired(false);
    }
}
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Core;

public class BursaryConfiguration : IEntityTypeConfiguration<Bursary>
{
    public void Configure(EntityTypeBuilder<Bursary> builder)
    {
        builder.ToTable("Bursaries");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.OwnsOne(b => b.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200);
            address.Property(a => a.City).HasMaxLength(100);
            address.Property(a => a.State).HasMaxLength(100);
            address.Property(a => a.Country).HasMaxLength(100);
            address.Property(a => a.ZipCode).HasMaxLength(20);
        });

        builder.HasMany(b => b.Schools)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "BursarySchool",
                j => j.HasOne<School>().WithMany().HasForeignKey("SchoolId"),
                j => j.HasOne<Bursary>().WithMany().HasForeignKey("BursaryId")
            );

        builder.HasMany(b => b.Payments)
            .WithOne(p => p.Bursary)
            .HasForeignKey(p => p.BursaryId);

        builder.Property(b => b.CreatedOn)
            .IsRequired();

        builder.Property(b => b.ModifiedOn)
            .IsRequired(false);
    }
}
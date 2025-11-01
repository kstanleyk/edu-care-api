using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Core;

public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.ToTable("AcademicYears");

        builder.HasKey(ay => ay.Id);
        builder.Property(ay => ay.Id).ValueGeneratedNever();

        builder.Property(ay => ay.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ay => ay.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ay => ay.StartDate)
            .IsRequired();

        builder.Property(ay => ay.EndDate)
            .IsRequired();

        builder.Property(ay => ay.IsCurrent)
            .IsRequired();

        builder.HasMany(ay => ay.Classes)
            .WithOne()
            .HasForeignKey(c => c.AcademicYearId);

        builder.Property(ay => ay.CreatedOn)
            .IsRequired();

        builder.Property(ay => ay.ModifiedOn)
            .IsRequired(false);
    }
}
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Core;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Classes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.GradeLevel)
            .IsRequired();

        // Note: Removed the FeeStructures collection mapping
        // since FeeStructure is now an aggregate root with its own table

        builder.Property(c => c.CreatedOn)
            .IsRequired();

        builder.Property(c => c.ModifiedOn)
            .IsRequired(false);

        // Index for efficient querying by academic year
        builder.HasIndex(c => c.AcademicYearId);

        // Unique constraint on code within academic year
        builder.HasIndex(c => new { c.AcademicYearId, c.Code })
            .IsUnique();

        // Index for querying by grade level
        builder.HasIndex(c => new { c.AcademicYearId, c.GradeLevel });
    }
}
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Core;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.EnrollmentDate)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId);

        builder.HasOne(e => e.Class)
            .WithMany()
            .HasForeignKey(e => e.ClassId);

        builder.HasOne(e => e.AcademicYear)
            .WithMany()
            .HasForeignKey(e => e.AcademicYearId);

        builder.HasOne(e => e.FeeStructure)
            .WithMany()
            .HasForeignKey(e => e.FeeStructureId);

        builder.HasMany(e => e.Scholarships)
            .WithOne(s => s.Enrollment)
            .HasForeignKey(s => s.EnrollmentId);

        builder.HasMany(e => e.Payments)
            .WithOne(p => p.Enrollment)
            .HasForeignKey(p => p.EnrollmentId);

        builder.HasMany(e => e.SelectedOptionalFees)
            .WithOne(efi => efi.Enrollment)
            .HasForeignKey(efi => efi.EnrollmentId);

        builder.Property(e => e.CreatedOn)
            .IsRequired();

        builder.Property(e => e.ModifiedOn)
            .IsRequired(false);

        builder.HasIndex(e => new { e.StudentId, e.AcademicYearId })
            .IsUnique();
    }
}
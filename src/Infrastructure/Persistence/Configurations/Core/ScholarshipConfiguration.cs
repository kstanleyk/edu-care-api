using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Core;

public class ScholarshipConfiguration : IEntityTypeConfiguration<Scholarship>
{
    public void Configure(EntityTypeBuilder<Scholarship> builder)
    {
        builder.ToTable("Scholarships");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.Percentage)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.IsActive)
            .IsRequired();

        builder.HasOne(s => s.Enrollment)
            .WithMany(e => e.Scholarships)
            .HasForeignKey(s => s.EnrollmentId);

        builder.Property(s => s.CreatedOn)
            .IsRequired();

        builder.Property(s => s.ModifiedOn)
            .IsRequired(false);
    }
}
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Core;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.StudentId)
            .IsRequired()
            .HasMaxLength(50);

        builder.OwnsOne(s => s.Name, name =>
        {
            name.Property(n => n.FirstName).HasMaxLength(100).IsRequired();
            name.Property(n => n.MiddleName).HasMaxLength(100).IsRequired(false);
            name.Property(n => n.LastName).HasMaxLength(100).IsRequired();
        });

        builder.Property(s => s.DateOfBirth)
            .IsRequired();

        builder.Property(s => s.Gender)
            .HasMaxLength(20)
            .IsRequired(false);

        builder.HasMany(s => s.Parents)
            .WithMany(p => p.Students)
            .UsingEntity<Dictionary<string, object>>(
                "StudentParent",
                j => j.HasOne<Parent>().WithMany().HasForeignKey("ParentId"),
                j => j.HasOne<Student>().WithMany().HasForeignKey("StudentId")
            );

        builder.HasMany(s => s.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId);

        builder.Property(s => s.CreatedOn)
            .IsRequired();

        builder.Property(s => s.ModifiedOn)
            .IsRequired(false);

        builder.HasIndex(s => s.StudentId)
            .IsUnique();
    }
}
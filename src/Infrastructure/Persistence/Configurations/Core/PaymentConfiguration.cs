using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Core;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.OwnsOne(p => p.Amount, amount =>
        {
            amount.Property(a => a.Amount).IsRequired().HasColumnType("decimal(18,2)");
            amount.Property(a => a.Currency).IsRequired().HasMaxLength(3);
        });

        builder.Property(p => p.PaymentDate)
            .IsRequired();

        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.HasOne(p => p.Enrollment)
            .WithMany()
            .HasForeignKey(p => p.EnrollmentId);

        builder.HasOne(p => p.Bursary)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BursaryId);

        builder.Property(p => p.CreatedOn)
            .IsRequired();

        builder.Property(p => p.ModifiedOn)
            .IsRequired(false);

        builder.HasIndex(p => p.ReferenceNumber)
            .IsUnique();
    }
}
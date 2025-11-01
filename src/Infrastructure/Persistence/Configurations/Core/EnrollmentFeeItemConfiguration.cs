using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Core;

public class EnrollmentFeeItemConfiguration : IEntityTypeConfiguration<EnrollmentFeeItem>
{
    public void Configure(EntityTypeBuilder<EnrollmentFeeItem> builder)
    {
        builder.ToTable("EnrollmentFeeItems");

        builder.HasKey(efi => efi.Id);
        builder.Property(efi => efi.Id).ValueGeneratedNever();

        builder.OwnsOne(efi => efi.Amount, amount =>
        {
            amount.Property(a => a.Amount).IsRequired().HasColumnType("decimal(18,2)");
            amount.Property(a => a.Currency).IsRequired().HasMaxLength(3);
        });

        builder.HasOne(efi => efi.Enrollment)
            .WithMany(e => e.SelectedOptionalFees)
            .HasForeignKey(efi => efi.EnrollmentId);

        builder.HasOne(efi => efi.FeeItem)
            .WithMany()
            .HasForeignKey(efi => efi.FeeItemId);

        builder.Property(efi => efi.CreatedOn)
            .IsRequired();

        builder.Property(efi => efi.ModifiedOn)
            .IsRequired(false);

        builder.HasIndex(efi => new { efi.EnrollmentId, efi.FeeItemId })
            .IsUnique();
    }
}
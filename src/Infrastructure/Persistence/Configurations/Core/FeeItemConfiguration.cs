using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Core;

public class FeeItemConfiguration : IEntityTypeConfiguration<FeeItem>
{
    public void Configure(EntityTypeBuilder<FeeItem> builder)
    {
        builder.ToTable("FeeItems");

        builder.HasKey(fi => fi.Id);
        builder.Property(fi => fi.Id).ValueGeneratedNever();

        builder.Property(fi => fi.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(fi => fi.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(fi => fi.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(fi => fi.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(fi => fi.IsActive)
            .IsRequired();

        builder.Property(fi => fi.CreatedOn)
            .IsRequired();

        builder.Property(fi => fi.ModifiedOn)
            .IsRequired(false);

        builder.HasIndex(fi => fi.Code)
            .IsUnique();
    }
}
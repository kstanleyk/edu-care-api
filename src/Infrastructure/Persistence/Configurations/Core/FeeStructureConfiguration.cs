using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCare.Infrastructure.Persistence.Configurations.Core;

public class FeeStructureConfiguration : IEntityTypeConfiguration<FeeStructure>
{
    public void Configure(EntityTypeBuilder<FeeStructure> builder)
    {
        builder.ToTable("FeeStructures");

        builder.HasKey(fs => fs.Id);
        builder.Property(fs => fs.Id).ValueGeneratedNever();

        builder.Property(fs => fs.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(fs => fs.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(fs => fs.IsActive)
            .IsRequired();

        builder.Property(fs => fs.EffectiveFrom)
            .IsRequired();

        builder.Property(fs => fs.EffectiveTo)
            .IsRequired(false);

        builder.HasOne(fs => fs.Class)
            .WithMany()
            .HasForeignKey(fs => fs.ClassId);

        builder.HasMany(fs => fs.FeeItems)
            .WithOne(fi => fi.FeeStructure)
            .HasForeignKey(fi => fi.FeeStructureId);

        builder.Property(fs => fs.CreatedOn)
            .IsRequired();

        builder.Property(fs => fs.ModifiedOn)
            .IsRequired(false);

        builder.HasIndex(fs => new { fs.ClassId, fs.IsActive })
            .HasFilter("[IsActive] = 1");
    }
}
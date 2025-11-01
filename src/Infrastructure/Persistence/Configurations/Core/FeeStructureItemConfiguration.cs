using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduCare.Infrastructure.Persistence.Configurations.Core;

public class FeeStructureItemConfiguration : IEntityTypeConfiguration<FeeStructureItem>
{
    public void Configure(EntityTypeBuilder<FeeStructureItem> builder)
    {
        builder.ToTable("FeeStructureItems");

        builder.HasKey(fi => fi.Id);
        builder.Property(fi => fi.Id).ValueGeneratedNever();

        builder.OwnsOne(fi => fi.Amount, amount =>
        {
            amount.Property(a => a.Amount).IsRequired().HasColumnType("decimal(18,2)");
            amount.Property(a => a.Currency).IsRequired().HasMaxLength(3);
        });

        builder.Property(fi => fi.IsOptional)
            .IsRequired();

        builder.Property(fi => fi.DisplayOrder)
            .IsRequired();

        builder.HasOne(fi => fi.FeeStructure)
            .WithMany(fs => fs.FeeItems)
            .HasForeignKey(fi => fi.FeeStructureId);

        builder.HasOne(fi => fi.FeeItem)
            .WithMany()
            .HasForeignKey(fi => fi.FeeItemId);

        builder.Property(fi => fi.CreatedOn)
            .IsRequired();

        builder.Property(fi => fi.ModifiedOn)
            .IsRequired(false);

        builder.HasIndex(fi => new { fi.FeeStructureId, fi.FeeItemId })
            .IsUnique();
    }
}
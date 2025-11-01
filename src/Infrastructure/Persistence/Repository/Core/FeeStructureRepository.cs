using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class FeeStructureRepository(IDatabaseFactory databaseFactory)
    : DataRepository<FeeStructure, Guid>(databaseFactory), IFeeStructureRepository
{
    public async Task<FeeStructure?> GetActiveByClassIdAsync(Guid classId, DateTime effectiveDate)
    {
        return await DbSet
            .Include(fs => fs.FeeItems)
            .ThenInclude(fi => fi.FeeItem)
            .Include(fs => fs.Class)
            .Where(fs => fs.ClassId == classId)
            .Where(fs => fs.IsActive)
            .Where(fs => fs.EffectiveFrom <= effectiveDate)
            .Where(fs => fs.EffectiveTo == null || fs.EffectiveTo >= effectiveDate)
            .OrderByDescending(fs => fs.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    public async Task<FeeStructure?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .Include(fs => fs.FeeItems)
            .ThenInclude(fi => fi.FeeItem)
            .Include(fs => fs.Class)
            .FirstOrDefaultAsync(fs => fs.Id == id);
    }

    public async Task<List<FeeStructure>> GetByClassIdAsync(Guid classId)
    {
        return await DbSet
            .Include(fs => fs.FeeItems)
            .ThenInclude(fi => fi.FeeItem)
            .Where(fs => fs.ClassId == classId)
            .OrderByDescending(fs => fs.EffectiveFrom)
            .ToListAsync();
    }

}
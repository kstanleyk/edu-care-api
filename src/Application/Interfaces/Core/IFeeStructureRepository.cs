using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IFeeStructureRepository : IRepository<FeeStructure, Guid>
{
    Task<FeeStructure?> GetActiveByClassIdAsync(Guid classId, DateTime effectiveDate);
    Task<List<FeeStructure>> GetByClassIdAsync(Guid classId);
    Task<FeeStructure?> GetByIdWithFeeItemsAsync(Guid id);
}
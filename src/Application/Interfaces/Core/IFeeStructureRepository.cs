using EduCare.Application.Features.Core.FeeManagement.Commands;
using EduCare.Application.Helpers;
using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IFeeStructureRepository : IRepository<FeeStructure, Guid>
{
    Task<FeeStructure?> GetActiveByClassIdAsync(Guid classId, DateTime effectiveDate);
    Task<List<FeeStructure>> GetByClassIdAsync(Guid classId);
    Task<FeeStructure?> GetByIdWithFeeItemsAsync(Guid id);
    Task<RepositoryActionResult<FeeStructure>> AddFeeItemToStructureAsync(AddFeeItemToStructureParameters parameters);
    Task<RepositoryActionResult<FeeStructure>> CreateFeeStructureAsync(CreateFeeStructureParameters parameters);
    Task<RepositoryActionResult<FeeStructure>> RemoveFeeItemFromStructureAsync(RemoveFeeItemFromStructureParameters parameters);
    Task<RepositoryActionResult<FeeStructure>> UpdateFeeItemAmountAsync(UpdateFeeItemAmountParameters parameters);
    Task<bool> HasOverlappingFeeStructureAsync(Guid classId, DateTime effectiveFrom, DateTime? effectiveTo, Guid? excludeId = null);
    Task<RepositoryActionResult<FeeStructure>> UpdateFeeStructureAsync(UpdateFeeStructureParameters parameters);
}
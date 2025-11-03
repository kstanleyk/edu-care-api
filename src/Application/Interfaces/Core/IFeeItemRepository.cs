using EduCare.Application.Features.Core.FeeManagement.Commands;
using EduCare.Application.Helpers;
using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IFeeItemRepository : IRepository<FeeItem, Guid>
{
    Task<FeeItem?> GetByCodeAsync(string code);
    Task<RepositoryActionResult<FeeItem>> CreateFeeItemAsync(CreateFeeItemParameters parameters);
    Task<RepositoryActionResult<FeeItem>> UpdateFeeItemAsync(UpdateFeeItemParameters parameters);
}
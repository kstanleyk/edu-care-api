using EduCare.Application.Features.Core.OrganizationManagement.Commands;
using EduCare.Application.Helpers;
using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IOrganizationRepository : IRepository<Organization, Guid>
{
    Task<Organization?> GetByIdWithDetailsAsync(Guid id);
    Task<Organization?> GetByCodeAsync(string code);
    Task<Organization?> GetByIdWithSchoolsAsync(Guid id);
    Task<RepositoryActionResult<Organization>> AddSchoolToOrganizationAsync(AddSchoolToOrganizationParameters parameters);
}
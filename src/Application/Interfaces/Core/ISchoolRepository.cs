using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface ISchoolRepository : IRepository<School, Guid>
{
    Task<List<School>> GetByOrganizationIdAsync(Guid organizationId);
    Task<List<School>> GetByOrganizationIdWithDetailsAsync(Guid organizationId);
    Task<School?> GetByCodeAsync(string code);
}
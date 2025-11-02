using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IOrganizationRepository : IRepository<Organization, Guid>
{
    Task<Organization?> GetByIdWithDetailsAsync(Guid id);
    Task<Organization?> GetByIdAsync(Guid id);
    Task<Organization?> GetByCodeAsync(string code);
    Task<List<Organization>> GetAllAsync();
}
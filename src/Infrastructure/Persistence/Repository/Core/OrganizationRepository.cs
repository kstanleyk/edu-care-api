using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class OrganizationRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Organization, Guid>(databaseFactory), IOrganizationRepository
{

}
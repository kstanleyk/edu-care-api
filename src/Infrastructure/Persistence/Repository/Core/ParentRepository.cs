using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class ParentRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Parent, Guid>(databaseFactory), IParentRepository
{

}
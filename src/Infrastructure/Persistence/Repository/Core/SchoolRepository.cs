using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class SchoolRepository(IDatabaseFactory databaseFactory)
    : DataRepository<School, Guid>(databaseFactory), ISchoolRepository
{

}
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class ScholarshipRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Scholarship, Guid>(databaseFactory), IScholarshipRepository
{

}
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class AcademicYearRepository(IDatabaseFactory databaseFactory)
    : DataRepository<AcademicYear, Guid>(databaseFactory), IAcademicYearRepository
{

}
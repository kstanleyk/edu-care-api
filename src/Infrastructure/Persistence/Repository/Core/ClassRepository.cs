using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class ClassRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Class, Guid>(databaseFactory), IClassRepository
{

}
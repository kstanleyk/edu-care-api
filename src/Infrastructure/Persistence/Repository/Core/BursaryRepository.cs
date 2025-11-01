using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class BursaryRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Bursary, Guid>(databaseFactory), IBursaryRepository
{

}
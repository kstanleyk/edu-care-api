using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class EnrollmentFeeItemRepository(IDatabaseFactory databaseFactory)
    : DataRepository<EnrollmentFeeItem, Guid>(databaseFactory), IEnrollmentFeeItemRepository
{

}
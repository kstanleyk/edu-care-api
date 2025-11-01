using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class FeeItemRepository(IDatabaseFactory databaseFactory)
    : DataRepository<FeeItem, Guid>(databaseFactory), IFeeItemRepository
{

}
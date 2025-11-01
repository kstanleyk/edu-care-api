using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class FeeStructureItemRepository(IDatabaseFactory databaseFactory)
    : DataRepository<FeeStructureItem, Guid>(databaseFactory), IFeeStructureItemRepository
{

}
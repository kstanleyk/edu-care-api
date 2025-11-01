using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class FeeStructureRepository(IDatabaseFactory databaseFactory)
    : DataRepository<FeeStructure, Guid>(databaseFactory), IFeeStructureRepository
{

}
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class PaymentRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Payment, Guid>(databaseFactory), IPaymentRepository
{

}
using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IPaymentRepository : IRepository<Payment, Guid>
{
    
}
using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IPaymentRepository : IRepository<Payment, Guid>
{
    Task<List<Payment>> GetByBursaryAndDateRangeAsync(Guid bursaryId, DateOnly fromDate, DateOnly toDate);
    Task<Payment?> GetByIdAsync(Guid id);
    Task<List<Payment>> GetByEnrollmentAsync(Guid enrollmentId);
    Task<List<Payment>> GetByReferenceNumberAsync(string referenceNumber);
    Task<Payment?> GetByIdWithFullDetailsAsync(Guid id);
}
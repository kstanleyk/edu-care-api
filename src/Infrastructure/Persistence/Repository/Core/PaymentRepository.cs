using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class PaymentRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Payment, Guid>(databaseFactory), IPaymentRepository
{
    public async Task<List<Payment>> GetByBursaryAndDateRangeAsync(Guid bursaryId, DateOnly fromDate, DateOnly toDate)
    {
        var fromDateTime = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDateTime = toDate.ToDateTime(TimeOnly.MaxValue);

        return await DbSet
            .Include(p => p.Bursary)
            .Include(p => p.Enrollment)
            .ThenInclude(e => e.Student)
            .Where(p => p.BursaryId == bursaryId)
            .Where(p => p.PaymentDate >= fromDateTime && p.PaymentDate <= toDateTime)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<Payment?> GetByIdWithFullDetailsAsync(Guid id)
    {
        return await DbSet
            .Include(p => p.Bursary)
            .Include(p => p.Enrollment)
            .ThenInclude(e => e.Student)
            .Include(p => p.Enrollment)
            .ThenInclude(e => e.Class)
            .Include(p => p.Enrollment)
            .ThenInclude(e => e.AcademicYear)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .Include(p => p.Bursary)
            .Include(p => p.Enrollment)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Payment>> GetByEnrollmentAsync(Guid enrollmentId)
    {
        return await DbSet
            .Include(p => p.Bursary)
            .Where(p => p.EnrollmentId == enrollmentId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetByReferenceNumberAsync(string referenceNumber)
    {
        return await DbSet
            .Include(p => p.Bursary)
            .Include(p => p.Enrollment)
            .Where(p => p.ReferenceNumber == referenceNumber)
            .ToListAsync();
    }
}
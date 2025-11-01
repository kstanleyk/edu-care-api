using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class EnrollmentRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Enrollment, Guid>(databaseFactory), IEnrollmentRepository
{
    public async Task<Enrollment?> GetByIdWithDetailsAsync(Guid enrollmentId)
    {
        return await DbSet
            .Include(e => e.Student)
            .Include(e => e.Class)
            .Include(e => e.AcademicYear)
            .Include(e => e.FeeStructure)
            .ThenInclude(fs => fs.FeeItems)
            .ThenInclude(fi => fi.FeeItem)
            .Include(e => e.Scholarships)
            .Include(e => e.Payments)
            .Include(e => e.SelectedOptionalFees)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);
    }

    public async Task<List<Enrollment>> GetStudentEnrollmentsWithDetailsAsync(Guid studentId)
    {
        return await DbSet
            .Include(e => e.Student)
            .Include(e => e.Class)
            .Include(e => e.AcademicYear)
            .Include(e => e.FeeStructure)
            .ThenInclude(fs => fs.FeeItems)
            .ThenInclude(fi => fi.FeeItem)
            .Include(e => e.Scholarships)
            .Include(e => e.Payments)
            .Include(e => e.SelectedOptionalFees)
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync();
    }
}
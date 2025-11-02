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

    public async Task<List<Enrollment>> GetEnrollmentsByAcademicYearIdAsync(Guid academicYearId)
    {
        return await DbSet
            .Where(e => e.AcademicYearId == academicYearId)
            .Include(e => e.Student!)
            .ThenInclude(s => s.Parents)
            .Include(e => e.Class)
            .Include(e => e.AcademicYear)
            .Include(e => e.FeeStructure!)
            .ThenInclude(fs => fs.FeeItems)
            .ThenInclude(fsi => fsi.FeeItem)
            .Include(e => e.Scholarships)
            .Include(e => e.Payments)
            .ThenInclude(p => p.Bursary)
            .Include(e => e.SelectedOptionalFees)
            .ThenInclude(efi => efi.FeeItem)
            .OrderBy(e => e.Class!.Name)
            .ThenBy(e => e.Student!.Name.LastName)
            .ThenBy(e => e.Student!.Name.FirstName)
            .ToListAsync();
    }

    public async Task<List<Enrollment>> GetActiveEnrollmentsByClassIdAsync(Guid classId)
    {
        return await DbSet
            .Where(e => e.ClassId == classId && e.IsActive)
            .Include(e => e.Student!)
            .ThenInclude(s => s.Parents)
            .Include(e => e.Student!)
            .ThenInclude(s => s.Enrollments)
            .ThenInclude(en => en.Class)
            .Include(e => e.Student!)
            .ThenInclude(s => s.Enrollments)
            .ThenInclude(en => en.AcademicYear)
            .Include(e => e.Class)
            .Include(e => e.AcademicYear)
            .OrderBy(e => e.Student!.Name.LastName)
            .ThenBy(e => e.Student!.Name.FirstName)
            .ToListAsync();
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

    public async Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(Guid studentId)
    {
        return await DbSet
            .Where(e => e.StudentId == studentId)
            .Include(e => e.Student!)
            .ThenInclude(s => s.Parents)
            .Include(e => e.Class)
            .Include(e => e.AcademicYear)
            .Include(e => e.FeeStructure!)
            .ThenInclude(fs => fs.FeeItems)
            .ThenInclude(fsi => fsi.FeeItem)
            .Include(e => e.Scholarships)
            .Include(e => e.Payments)
            .ThenInclude(p => p.Bursary)
            .Include(e => e.SelectedOptionalFees)
            .ThenInclude(efi => efi.FeeItem)
            .OrderByDescending(e => e.EnrollmentDate)
            .ThenByDescending(e => e.IsActive)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetActiveEnrollmentAsync(Guid studentId, Guid academicYearId)
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
            .FirstOrDefaultAsync(e => e.StudentId == studentId &&
                                      e.AcademicYearId == academicYearId &&
                                      e.IsActive);
    }
}
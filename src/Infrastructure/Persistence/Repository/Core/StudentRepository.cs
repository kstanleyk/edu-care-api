using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class StudentRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Student, Guid>(databaseFactory), IStudentRepository
{
    public async Task<Student?> GetByIdWithDetailsAsync(Guid id)
    {
        return await DbSet
            .Include(s => s.Parents)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Class)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.AcademicYear)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByStudentIdWithDetailsAsync(string studentId)
    {
        return await DbSet
            .Include(s => s.Parents)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Class)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.AcademicYear)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.FeeStructure)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Scholarships)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Payments)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);
    }

    public async Task<Student?> GetByStudentIdAsync(string studentId)
    {
        return await DbSet
            .Include(s => s.Parents)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);
    }

    public async Task<Student?> GetByIdWithCurrentEnrollmentAsync(Guid studentId)
    {
        return await DbSet
            .Include(s => s.Parents)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Class)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.AcademicYear)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.FeeStructure)
            .ThenInclude(fs => fs!.FeeItems)
            .ThenInclude(fsi => fsi.FeeItem)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Scholarships)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Payments)
            .ThenInclude(p => p.Bursary)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.SelectedOptionalFees)
            .ThenInclude(efi => efi.FeeItem)
            .FirstOrDefaultAsync(s => s.Id == studentId);
    }

    public async Task<bool> ExistsAsync(Guid studentId)
    {
        return await DbSet
            .AnyAsync(s => s.Id == studentId);
    }

    public async Task<List<Student>> GetByClassIdAsync(Guid classId)
    {
        return await DbSet
            .Include(s => s.Enrollments)
            .Where(s => s.Enrollments.Any(e => e.ClassId == classId && e.IsActive))
            .ToListAsync();
    }
}
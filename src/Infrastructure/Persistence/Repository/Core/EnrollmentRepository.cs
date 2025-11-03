using EduCare.Application.Features.Core.EnrollmentManagement.Commands;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using static EduCare.Application.Features.Core.EnrollmentManagement.Commands.PromoteStudentCommandHandler;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class EnrollmentRepository(IStudentRepository studentRepository, IClassRepository classRepository,IAcademicYearRepository academicYearRepository, IFeeStructureRepository feeStructureRepository, IDatabaseFactory databaseFactory)
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
            .Include(e => e.Student)
            .ThenInclude(s => s.Parents)
            .Include(e => e.Class)
            .Include(e => e.AcademicYear)
            .Include(e => e.FeeStructure)
            .ThenInclude(fs => fs.FeeItems)
            .ThenInclude(fsi => fsi.FeeItem)
            .Include(e => e.Scholarships)
            .Include(e => e.Payments)
            .ThenInclude(p => p.Bursary)
            .Include(e => e.SelectedOptionalFees)
            .ThenInclude(efi => efi.FeeItem)
            .OrderBy(e => e.Class.Name)
            .ThenBy(e => e.Student.Name.LastName)
            .ThenBy(e => e.Student.Name.FirstName)
            .ToListAsync();
    }

    public async Task<RepositoryActionResult<Enrollment>> MarkEnrollmentInactiveAsync(MarkEnrollmentInactiveParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the enrollment with all related data
            var enrollment = await GetByIdWithDetailsAsync(parameters.EnrollmentId);
            if (enrollment is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.NotFound);
            }

            // Check if enrollment is already inactive
            if (!enrollment.IsActive)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Invalid);
            }

            // Check if there are any outstanding payments
            var balance = enrollment.CalculateBalance();
            if (balance.Amount > 0)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Invalid);
            }

            // Mark enrollment as inactive using domain method
            enrollment.MarkAsInactive();

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<Enrollment>(enrollment, RepositoryActionStatus.Updated);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<List<Enrollment>> GetActiveEnrollmentsByClassIdAsync(Guid classId)
    {
        return await DbSet
            .Where(e => e.ClassId == classId && e.IsActive)
            .Include(e => e.Student)
            .ThenInclude(s => s.Parents)
            .Include(e => e.Student)
            .ThenInclude(s => s.Enrollments)
            .ThenInclude(en => en.Class)
            .Include(e => e.Student)
            .ThenInclude(s => s.Enrollments)
            .ThenInclude(en => en.AcademicYear)
            .Include(e => e.Class)
            .Include(e => e.AcademicYear)
            .OrderBy(e => e.Student.Name.LastName)
            .ThenBy(e => e.Student.Name.FirstName)
            .ToListAsync();
    }

    public async Task<RepositoryActionResult<Enrollment>> PromoteStudentAsync(PromoteStudentParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the current enrollment with all details
            var currentEnrollment = await GetByIdWithDetailsAsync(parameters.EnrollmentId);
            if (currentEnrollment is null || !currentEnrollment.IsActive)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.NotFound);
            }

            // Check if student is already enrolled in the next academic year
            var existingEnrollment = await GetActiveEnrollmentByStudentAndAcademicYearAsync(
                currentEnrollment.StudentId, parameters.NextAcademicYearId);

            if (existingEnrollment is not null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Invalid);
            }

            // Validate related entities exist
            var nextClass = await classRepository.GetByIdAsync(parameters.NextClassId);

            var nextAcademicYear = await academicYearRepository.GetByIdAsync(parameters.NextAcademicYearId);

            var newFeeStructure = await feeStructureRepository.GetByIdAsync(parameters.NewFeeStructureId);

            if (nextClass is null || nextAcademicYear is null || newFeeStructure is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.NotFound);
            }

            // Validate next class belongs to next academic year
            if (nextClass.AcademicYearId != parameters.NextAcademicYearId)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Invalid);
            }

            // Validate new fee structure belongs to next class
            if (newFeeStructure.ClassId != parameters.NextClassId)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Invalid);
            }

            // Check if there are any outstanding payments in current enrollment
            var currentBalance = currentEnrollment.CalculateBalance();
            if (currentBalance.Amount > 0)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Invalid);
            }

            // Mark current enrollment as inactive
            currentEnrollment.MarkAsInactive();

            // Create new enrollment for the next class/academic year
            var newEnrollment = Enrollment.Create(
                currentEnrollment.StudentId,
                parameters.NextClassId,
                parameters.NextAcademicYearId,
                parameters.NewFeeStructureId,
                DateOnly.FromDateTime(DateTime.UtcNow)); // Use current date for new enrollment

            // Copy scholarships from current enrollment to new enrollment if applicable
            // (This is a business decision - you might want to copy active scholarships)
            foreach (var scholarship in currentEnrollment.Scholarships.Where(s => s.IsActive))
            {
                // Create a new scholarship for the new enrollment
                var newScholarship = Scholarship.Create(
                    newEnrollment.Id,
                    scholarship.Type,
                    scholarship.Percentage,
                    scholarship.Description + " (Carried over from previous enrollment)",
                    scholarship.IsActive);

                newEnrollment.AddScholarship(newScholarship);
            }

            // Add new enrollment to context
            await DbSet.AddAsync(newEnrollment);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<Enrollment>(newEnrollment, RepositoryActionStatus.Created);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Error, ex);
        }
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
            .Include(e => e.Student)
            .ThenInclude(s => s.Parents)
            .Include(e => e.Class)
            .Include(e => e.AcademicYear)
            .Include(e => e.FeeStructure)
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

    public async Task<Enrollment?> GetActiveEnrollmentByStudentAndAcademicYearAsync(Guid studentId, Guid academicYearId)
    {
        return await DbSet
            .Include(e => e.AcademicYear)
            .FirstOrDefaultAsync(e => e.StudentId == studentId &&
                                      e.AcademicYearId == academicYearId &&
                                      e.IsActive);
    }

    public async Task<RepositoryActionResult<Enrollment>> EnrollStudentAsync(EnrollStudentParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Check if student is already enrolled in the same academic year
            var existingEnrollment = await GetActiveEnrollmentByStudentAndAcademicYearAsync(
                parameters.StudentId, parameters.AcademicYearId);

            if (existingEnrollment is not null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Invalid);
            }

            // Validate related entities exist
            var student = await studentRepository.GetByIdAsync(parameters.StudentId);

            var classEntity = await classRepository.GetByIdAsync(parameters.ClassId);

            var academicYear = await academicYearRepository.GetByIdAsync(parameters.AcademicYearId);// await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.Id == parameters.AcademicYearId);
            var feeStructure = await feeStructureRepository.GetByIdAsync(parameters.FeeStructureId);//  _context.FeeStructures.FirstOrDefaultAsync(fs => fs.Id == parameters.FeeStructureId);

            if (student is null || classEntity is null || academicYear is null || feeStructure is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.NotFound);
            }

            // Validate class belongs to academic year
            if (classEntity.AcademicYearId != parameters.AcademicYearId)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Invalid);
            }

            // Validate fee structure belongs to class
            if (feeStructure.ClassId != parameters.ClassId)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Invalid);
            }

            // Create enrollment using domain factory method
            var enrollment = Enrollment.Create(
                parameters.StudentId,
                parameters.ClassId,
                parameters.AcademicYearId,
                parameters.FeeStructureId,
                parameters.EnrollmentDate);

            // Add to context
            await DbSet.AddAsync(enrollment);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<Enrollment>(enrollment, RepositoryActionStatus.Created);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Enrollment>(null, RepositoryActionStatus.Error, ex);
        }
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
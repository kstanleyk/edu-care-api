using EduCare.Application.Features.Core.BursaryManagement.Dtos;
using EduCare.Application.Features.Core.EnrollmentManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Queries;

public record GetStudentFeeSummaryQuery(Guid StudentId) : IRequest<Result<StudentFeeSummaryDto>>;

public class GetStudentFeeSummaryQueryHandler(
    IStudentRepository studentRepository,
    IEnrollmentRepository enrollmentRepository)
    : IRequestHandler<GetStudentFeeSummaryQuery, Result<StudentFeeSummaryDto>>
{
    public async Task<Result<StudentFeeSummaryDto>> Handle(GetStudentFeeSummaryQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate student exists
            var student = await studentRepository.GetByIdWithDetailsAsync(query.StudentId);
            if (student is null)
            {
                return Result<StudentFeeSummaryDto>.Failed(
                    Error.NotFound(
                        "Student.NotFound",
                        $"Student with ID {query.StudentId} was not found"
                    )
                );
            }

            // Get all enrollments for the student
            var enrollments = await enrollmentRepository.GetStudentEnrollmentsWithDetailsAsync(query.StudentId);

            if (!enrollments.Any())
            {
                return Result<StudentFeeSummaryDto>.Failed(
                    Error.Validation(
                        "Student.NoEnrollments",
                        $"Student with ID {query.StudentId} has no enrollments"
                    )
                );
            }

            // Calculate enrollment balances and summary
            var enrollmentBalances = CalculateEnrollmentBalances(enrollments);
            var currentEnrollment = enrollments.FirstOrDefault(e => e.IsActive);
            var totalBalance = CalculateTotalBalance(enrollmentBalances);
            var currentTermBalance = CalculateCurrentTermBalance(currentEnrollment);

            // Map to StudentFeeSummaryDto
            var studentFeeSummaryDto = new StudentFeeSummaryDto(
                StudentId: student.Id,
                StudentName: student.Name.FullName,
                StudentCode: student.StudentId,
                CurrentClass: currentEnrollment?.Class?.Name ?? "Not enrolled",
                CurrentClassCode: currentEnrollment?.Class?.Code ?? string.Empty,
                TotalBalance: totalBalance,
                CurrentTermBalance: currentTermBalance,
                EnrollmentBalances: enrollmentBalances
            );

            return Result<StudentFeeSummaryDto>.Succeeded(studentFeeSummaryDto);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting student fee summary for student {StudentId}", query.StudentId);

            return Result<StudentFeeSummaryDto>.Failed(
                Error.Failure(
                    "StudentFeeSummaryQuery.Failed",
                    $"An error occurred while retrieving the student fee summary: {ex.Message}"
                )
            );
        }
    }

    private static List<EnrollmentBalanceDto> CalculateEnrollmentBalances(List<Enrollment> enrollments)
    {
        var enrollmentBalances = new List<EnrollmentBalanceDto>();

        foreach (var enrollment in enrollments.OrderByDescending(e => e.EnrollmentDate))
        {
            var totalFees = enrollment.CalculateTotalFees();
            var totalPaid = enrollment.CalculateTotalPaid();
            var balance = enrollment.CalculateBalance();
            var scholarshipDiscount = CalculateScholarshipDiscount(enrollment, totalFees);

            var enrollmentBalance = new EnrollmentBalanceDto(
                EnrollmentId: enrollment.Id,
                AcademicYearId: enrollment.AcademicYearId,
                AcademicYearName: enrollment.AcademicYear?.Name ?? string.Empty,
                ClassId: enrollment.ClassId,
                ClassName: enrollment.Class?.Name ?? string.Empty,
                ClassCode: enrollment.Class?.Code ?? string.Empty,
                GradeLevel: enrollment.Class?.GradeLevel ?? 0,
                TotalFees: totalFees,
                TotalPaid: totalPaid,
                ScholarshipDiscount: scholarshipDiscount,
                Balance: balance,
                EnrollmentDate: enrollment.EnrollmentDate.ToDateTime(TimeOnly.MinValue),
                IsActive: enrollment.IsActive
            );

            enrollmentBalances.Add(enrollmentBalance);
        }

        return enrollmentBalances;
    }

    private static Money CalculateTotalBalance(List<EnrollmentBalanceDto> enrollmentBalances)
    {
        var totalBalance = enrollmentBalances.Sum(eb => eb.Balance.Amount);
        return new Money(totalBalance);
    }

    private static Money CalculateCurrentTermBalance(Enrollment? currentEnrollment)
    {
        if (currentEnrollment is null)
            return new Money(0);

        return currentEnrollment.CalculateBalance();
    }

    private static Money CalculateScholarshipDiscount(Enrollment enrollment, Money totalFees)
    {
        var activeScholarships = enrollment.Scholarships.Where(s => s.IsActive).ToArray();
        if (!activeScholarships.Any())
            return new Money(0);

        var totalPercentage = activeScholarships.Sum(s => s.Percentage);
        var maxPercentage = Math.Min(totalPercentage, 100);

        var discountAmount = totalFees.Amount * (maxPercentage / 100);
        return new Money(discountAmount);
    }
}
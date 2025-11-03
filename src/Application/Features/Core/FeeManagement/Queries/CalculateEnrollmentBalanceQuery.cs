using EduCare.Application.Features.Core.BursaryManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using EduCare.Domain.ValueObjects;
using MediatR;
using Error = EduCare.Application.Helpers.Error;

namespace EduCare.Application.Features.Core.FeeManagement.Queries;

public record CalculateEnrollmentBalanceQuery(Guid EnrollmentId) : IRequest<Result<BalanceDto>>;

public class CalculateEnrollmentBalanceQueryHandler(
    IEnrollmentRepository enrollmentRepository,
    IStudentRepository studentRepository,
    IClassRepository classRepository)
    : IRequestHandler<CalculateEnrollmentBalanceQuery, Result<BalanceDto>>
{
    public async Task<Result<BalanceDto>> Handle(CalculateEnrollmentBalanceQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate enrollment exists
            var enrollment = await enrollmentRepository.GetByIdWithDetailsAsync(query.EnrollmentId);
            if (enrollment is null)
            {
                return Result<BalanceDto>.Failed(
                    Error.NotFound(
                        "Enrollment.NotFound",
                        $"Enrollment with ID {query.EnrollmentId} was not found"
                    )
                );
            }

            // Validate enrollment is active
            if (!enrollment.IsActive)
            {
                return Result<BalanceDto>.Failed(
                    Error.Validation(
                        "Enrollment.Inactive",
                        $"Enrollment with ID {query.EnrollmentId} is not active"
                    )
                );
            }

            // Get additional context for validation
            var student = await studentRepository.GetByIdAsync(enrollment.StudentId);
            var classEntity = await classRepository.GetByIdAsync(enrollment.ClassId);

            if (student is null || classEntity is null)
            {
                return Result<BalanceDto>.Failed(
                    Error.NotFound(
                        "RelatedEntity.NotFound",
                        "Related student or class information not found"
                    )
                );
            }

            // Calculate financial details
            var totalFees = enrollment.CalculateTotalFees();
            var totalPaid = enrollment.CalculateTotalPaid();
            var balance = enrollment.CalculateBalance();
            var scholarshipDiscount = CalculateScholarshipDiscount(enrollment, totalFees);

            var balanceDto = new BalanceDto(
                totalFees,
                totalPaid,
                scholarshipDiscount,
                balance
            );

            return Result<BalanceDto>.Succeeded(balanceDto);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error calculating enrollment balance for enrollment {EnrollmentId}", query.EnrollmentId);

            return Result<BalanceDto>.Failed(
                Error.Failure(
                    "BalanceCalculation.Failed",
                    $"An error occurred while calculating the balance: {ex.Message}"
                )
            );
        }
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
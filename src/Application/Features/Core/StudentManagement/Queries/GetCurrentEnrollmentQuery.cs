using EduCare.Application.Features.Core.FeeManagement.Dtos;
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.StudentManagement.Queries;

public record GetCurrentEnrollmentQuery(Guid StudentId) : IRequest<Result<EnrollmentDto?>>;

public class GetCurrentEnrollmentQueryHandler(
    IStudentRepository studentRepository)
    : IRequestHandler<GetCurrentEnrollmentQuery, Result<EnrollmentDto?>>
{
    public async Task<Result<EnrollmentDto?>> Handle(GetCurrentEnrollmentQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (query.StudentId == Guid.Empty)
            {
                return Result<EnrollmentDto?>.Failed(
                    Error.Validation(
                        "StudentId.Empty",
                        "Student ID cannot be empty"
                    )
                );
            }

            // Get student with current enrollment and related details
            var student = await studentRepository.GetByIdWithCurrentEnrollmentAsync(query.StudentId);
            if (student is null)
            {
                return Result<EnrollmentDto?>.Failed(
                    Error.NotFound(
                        "Student.NotFound",
                        $"Student with ID '{query.StudentId}' was not found"
                    )
                );
            }

            // Get current enrollment
            var currentEnrollment = student.GetCurrentEnrollment();
            if (currentEnrollment is null)
            {
                return Result<EnrollmentDto?>.Succeeded(null);
            }

            // Manually map Enrollment to EnrollmentDto without AutoMapper
            var enrollmentDto = MapToEnrollmentDto(currentEnrollment);

            return Result<EnrollmentDto?>.Succeeded(enrollmentDto);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting current enrollment for student {StudentId}", query.StudentId);

            return Result<EnrollmentDto?>.Failed(
                Error.Failure(
                    "EnrollmentQuery.Failed",
                    $"An error occurred while retrieving the current enrollment: {ex.Message}"
                )
            );
        }
    }

    private static EnrollmentDto MapToEnrollmentDto(Enrollment enrollment)
    {
        return new EnrollmentDto(
            Id: enrollment.Id,
            StudentId: enrollment.StudentId,
            StudentName: enrollment.Student.Name.FullName,
            StudentCode: enrollment.Student.StudentId,
            ClassId: enrollment.ClassId,
            ClassName: enrollment.Class?.Name ?? string.Empty,
            ClassCode: enrollment.Class?.Code ?? string.Empty,
            AcademicYearId: enrollment.AcademicYearId,
            AcademicYearName: enrollment.AcademicYear?.Name ?? string.Empty,
            AcademicYearCode: enrollment.AcademicYear?.Code ?? string.Empty,
            FeeStructureId: enrollment.FeeStructureId,
            FeeStructureName: enrollment.FeeStructure?.Name ?? string.Empty,
            EnrollmentDate: enrollment.EnrollmentDate,
            IsActive: enrollment.IsActive,
            CreatedOn: enrollment.CreatedOn,
            ModifiedOn: enrollment.ModifiedOn,
            TotalFees: enrollment.CalculateTotalFees(),
            TotalPaid: enrollment.CalculateTotalPaid(),
            Balance: enrollment.CalculateBalance(),
            Scholarships: enrollment.Scholarships.Select(MapToScholarshipDto).ToList(),
            Payments: enrollment.Payments.Select(MapToPaymentDto).ToList(),
            SelectedOptionalFees: enrollment.SelectedOptionalFees.Select(MapToEnrollmentFeeItemDto).ToList()
        );
    }

    private static ScholarshipDto MapToScholarshipDto(Scholarship scholarship)
    {
        return new ScholarshipDto(
            Id: scholarship.Id,
            Type: scholarship.Type,
            Percentage: scholarship.Percentage,
            Description: scholarship.Description,
            IsActive: scholarship.IsActive,
            CreatedOn: scholarship.CreatedOn,
            ModifiedOn: scholarship.ModifiedOn
        );
    }

    private static PaymentDto MapToPaymentDto(Payment payment)
    {
        return new PaymentDto(
            Id: payment.Id,
            Amount: payment.Amount,
            PaymentDate: payment.PaymentDate,
            PaymentMethod: payment.PaymentMethod,
            ReferenceNumber: payment.ReferenceNumber,
            Notes: payment.Notes,
            BursaryId: payment.BursaryId,
            BursaryName: payment.Bursary?.Name ?? string.Empty,
            CreatedOn: payment.CreatedOn,
            ModifiedOn: payment.ModifiedOn
        );
    }

    private static EnrollmentFeeItemDto MapToEnrollmentFeeItemDto(EnrollmentFeeItem enrollmentFeeItem)
    {
        return new EnrollmentFeeItemDto(
            Id: enrollmentFeeItem.Id,
            FeeItemId: enrollmentFeeItem.FeeItemId,
            FeeItemName: enrollmentFeeItem.FeeItem?.Name ?? string.Empty,
            FeeItemCode: enrollmentFeeItem.FeeItem?.Code ?? string.Empty,
            Amount: enrollmentFeeItem.Amount,
            CreatedOn: enrollmentFeeItem.CreatedOn
        );
    }
}
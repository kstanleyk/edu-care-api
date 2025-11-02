using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.StudentManagement.Queries;

public record GetAcademicYearEnrollmentsQuery(Guid AcademicYearId) : IRequest<Result<List<EnrollmentDto>>>;

public class GetAcademicYearEnrollmentsQueryHandler(
    IEnrollmentRepository enrollmentRepository)
    : IRequestHandler<GetAcademicYearEnrollmentsQuery, Result<List<EnrollmentDto>>>
{
    public async Task<Result<List<EnrollmentDto>>> Handle(GetAcademicYearEnrollmentsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (query.AcademicYearId == Guid.Empty)
            {
                return Result<List<EnrollmentDto>>.Failed(
                    Error.Validation(
                        "AcademicYearId.Empty",
                        "Academic Year ID cannot be empty"
                    )
                );
            }

            // Get all enrollments for the academic year with related details
            var enrollments = await enrollmentRepository.GetEnrollmentsByAcademicYearIdAsync(query.AcademicYearId);

            if (!enrollments.Any())
            {
                return Result<List<EnrollmentDto>>.Succeeded([]);
            }

            // Manually map each enrollment to EnrollmentDto without AutoMapper
            var enrollmentDtos = enrollments
                .Select(MapToEnrollmentDto)
                .ToList();

            return Result<List<EnrollmentDto>>.Succeeded(enrollmentDtos);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting enrollments for academic year {AcademicYearId}", query.AcademicYearId);

            return Result<List<EnrollmentDto>>.Failed(
                Error.Failure(
                    "AcademicYearEnrollmentsQuery.Failed",
                    $"An error occurred while retrieving enrollments for the academic year: {ex.Message}"
                )
            );
        }
    }

    private static EnrollmentDto MapToEnrollmentDto(Enrollment enrollment)
    {
        return new EnrollmentDto(
            Id: enrollment.Id,
            StudentId: enrollment.StudentId,
            StudentName: enrollment.Student?.Name.FullName ?? string.Empty,
            StudentCode: enrollment.Student?.StudentId ?? string.Empty,
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
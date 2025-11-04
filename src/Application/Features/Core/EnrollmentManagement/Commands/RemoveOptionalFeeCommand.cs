using EduCare.Application.Features.Core.Dtos;
using EduCare.Application.Features.Core.EnrollmentManagement.Validators;
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Commands;

public record RemoveOptionalFeeCommand(Guid EnrollmentId, Guid FeeItemId) : IRequest<Result<EnrollmentDto>>;

public class RemoveOptionalFeeCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    IStudentRepository studentRepository,
    IClassRepository classRepository,
    IAcademicYearRepository academicYearRepository,
    IFeeStructureRepository feeStructureRepository)
    : IRequestHandler<RemoveOptionalFeeCommand, Result<EnrollmentDto>>
{
    public async Task<Result<EnrollmentDto>> Handle(RemoveOptionalFeeCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new RemoveOptionalFeeCommandValidator();
            var validationResult = await validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList();
                return Result<EnrollmentDto>.Failed(
                    Error.Validation("Enrollment.ValidationError", string.Join(", ", validationErrors))
                );
            }

            // Validate enrollment exists and get it with all related data
            var enrollment = await enrollmentRepository.GetByIdWithDetailsAsync(command.EnrollmentId);
            if (enrollment is null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.NotFound(
                        "Enrollment.NotFound",
                        $"Enrollment with ID '{command.EnrollmentId}' was not found"
                    )
                );
            }

            // Check if enrollment is active
            if (!enrollment.IsActive)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.NotActive",
                        "Cannot modify fees for an inactive enrollment"
                    ),
                    "Cannot remove optional fee from an inactive enrollment"
                );
            }

            // Check if the fee item exists in the selected optional fees
            var selectedFee = enrollment.SelectedOptionalFees
                .FirstOrDefault(sf => sf.FeeItemId == command.FeeItemId);

            if (selectedFee is null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.NotFound(
                        "FeeItem.NotSelected",
                        $"Fee item with ID '{command.FeeItemId}' is not selected for this enrollment"
                    ),
                    "The specified fee item is not selected for this enrollment"
                );
            }

            // Check if payments have already been made for this enrollment
            // If payments exist, we might want to prevent removal or handle it differently
            var totalPaid = enrollment.CalculateTotalPaid();
            if (totalPaid.Amount > 0)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.PaymentsExist",
                        "Cannot remove optional fee after payments have been made"
                    ),
                    "Cannot remove optional fee because payments have already been made for this enrollment"
                );
            }

            // Create parameters object
            var parameters = new RemoveOptionalFeeParameters(command.EnrollmentId, command.FeeItemId);

            // Call repository with transaction support
            var repositoryResult = await enrollmentRepository.RemoveOptionalFeeAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Updated)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.NotFound => Result<EnrollmentDto>.Failed(
                        Error.NotFound(
                            "Enrollment.NotFound",
                            $"Enrollment with ID '{command.EnrollmentId}' was not found"
                        ),
                        "Enrollment not found"
                    ),
                    RepositoryActionStatus.Invalid => Result<EnrollmentDto>.Failed(
                        Error.Validation(
                            "FeeItem.NotSelected",
                            $"Fee item is not selected for this enrollment"
                        ),
                        "The specified fee item is not selected for this enrollment"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.ConcurrencyConflict",
                            "A concurrency conflict occurred while removing the optional fee"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.NotUpdated",
                            "Optional fee was not removed"
                        ),
                        "No changes were made to the enrollment"
                    ),
                    _ => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.UpdateFailed",
                            $"An unexpected error occurred while removing the optional fee. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while updating the enrollment"
                    )
                };
            }

            // Get related entities for DTO mapping
            var student = await studentRepository.GetByIdAsync(enrollment.StudentId);
            var classEntity = await classRepository.GetByIdAsync(enrollment.ClassId);
            var academicYear = await academicYearRepository.GetByIdAsync(enrollment.AcademicYearId);
            var feeStructure = await feeStructureRepository.GetByIdWithFeeItemsAsync(enrollment.FeeStructureId);

            // Manually map to DTO
            var enrollmentDto = MapToEnrollmentDto(
                repositoryResult.Entity!,
                student!,
                classEntity!,
                academicYear!,
                feeStructure!);

            return Result<EnrollmentDto>.Succeeded(
                enrollmentDto,
                "Optional fee removed successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error removing optional fee {FeeItemId} from enrollment {EnrollmentId}", command.FeeItemId, command.EnrollmentId);

            return Result<EnrollmentDto>.Failed(
                Error.Failure(
                    "Enrollment.UpdateFailed",
                    $"An error occurred while removing the optional fee: {ex.Message}"
                ),
                "An unexpected error occurred while processing your request"
            );
        }
    }

    private static EnrollmentDto MapToEnrollmentDto(
        Enrollment enrollment,
        Student student,
        Class classEntity,
        AcademicYear academicYear,
        FeeStructure feeStructure)
    {
        return new EnrollmentDto(
            Id: enrollment.Id,
            StudentId: enrollment.StudentId,
            StudentName: student.Name.FullName,
            StudentCode: student.StudentId,
            ClassId: enrollment.ClassId,
            ClassName: classEntity.Name,
            ClassCode: classEntity.Code,
            AcademicYearId: enrollment.AcademicYearId,
            AcademicYearName: academicYear.Name,
            AcademicYearCode: academicYear.Code,
            FeeStructureId: enrollment.FeeStructureId,
            FeeStructureName: feeStructure.Name,
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
            BursaryName: payment.Bursary.Name,
            CreatedOn: payment.CreatedOn,
            ModifiedOn: payment.ModifiedOn
        );
    }

    private static EnrollmentFeeItemDto MapToEnrollmentFeeItemDto(EnrollmentFeeItem enrollmentFeeItem)
    {
        return new EnrollmentFeeItemDto(
            Id: enrollmentFeeItem.Id,
            FeeItemId: enrollmentFeeItem.FeeItemId,
            FeeItemName: enrollmentFeeItem.FeeItem.Name,
            FeeItemCode: enrollmentFeeItem.FeeItem.Code,
            Amount: enrollmentFeeItem.Amount,
            CreatedOn: enrollmentFeeItem.CreatedOn
        );
    }
}

public record RemoveOptionalFeeParameters(Guid EnrollmentId, Guid FeeItemId);

public record RemoveOptionalFeeRequestDto(Guid EnrollmentId, Guid FeeItemId);
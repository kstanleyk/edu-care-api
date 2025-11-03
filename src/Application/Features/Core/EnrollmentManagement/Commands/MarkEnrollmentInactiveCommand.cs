using EduCare.Application.Features.Core.Dtos;
using EduCare.Application.Features.Core.EnrollmentManagement.Validators;
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Commands;

public record MarkEnrollmentInactiveCommand(Guid EnrollmentId) : IRequest<Result<EnrollmentDto>>;

public class MarkEnrollmentInactiveCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    IStudentRepository studentRepository,
    IClassRepository classRepository,
    IAcademicYearRepository academicYearRepository,
    IFeeStructureRepository feeStructureRepository)
    : IRequestHandler<MarkEnrollmentInactiveCommand, Result<EnrollmentDto>>
{
    public async Task<Result<EnrollmentDto>> Handle(MarkEnrollmentInactiveCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new MarkEnrollmentInactiveCommandValidator();
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

            // Check if enrollment is already inactive
            if (!enrollment.IsActive)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.AlreadyInactive",
                        "Enrollment is already marked as inactive"
                    ),
                    "Enrollment is already inactive"
                );
            }

            // Check if there are any outstanding payments
            var balance = enrollment.CalculateBalance();
            if (balance.Amount > 0)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.OutstandingBalance",
                        $"Cannot mark enrollment as inactive with outstanding balance of {balance.Amount} {balance.Currency}"
                    ),
                    "Cannot mark enrollment as inactive while there is an outstanding balance"
                );
            }

            // Create parameters object
            var parameters = new MarkEnrollmentInactiveParameters(command.EnrollmentId);

            // Call repository with transaction support
            var repositoryResult = await enrollmentRepository.MarkEnrollmentInactiveAsync(parameters);

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
                            "Enrollment.AlreadyInactive",
                            "Enrollment is already inactive"
                        ),
                        "Enrollment is already inactive"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.ConcurrencyConflict",
                            "A concurrency conflict occurred while marking the enrollment as inactive"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.NotUpdated",
                            "Enrollment was not updated"
                        ),
                        "No changes were made to the enrollment"
                    ),
                    _ => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.UpdateFailed",
                            $"An unexpected error occurred while marking the enrollment as inactive. Status: {repositoryResult.Status}"
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
                "Enrollment marked as inactive successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error marking enrollment {EnrollmentId} as inactive", command.EnrollmentId);

            return Result<EnrollmentDto>.Failed(
                Error.Failure(
                    "Enrollment.UpdateFailed",
                    $"An error occurred while marking the enrollment as inactive: {ex.Message}"
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

public record MarkEnrollmentInactiveRequestDto(Guid EnrollmentId);

public record MarkEnrollmentInactiveParameters(Guid EnrollmentId);
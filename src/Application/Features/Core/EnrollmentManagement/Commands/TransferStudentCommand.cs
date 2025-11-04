using EduCare.Application.Features.Core.Dtos;
using EduCare.Application.Features.Core.EnrollmentManagement.Validators;
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Commands;

public record TransferStudentCommand(
    Guid EnrollmentId,
    Guid NewClassId,
    Guid NewFeeStructureId) : IRequest<Result<EnrollmentDto>>;

public class TransferStudentCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    IStudentRepository studentRepository,
    IClassRepository classRepository,
    IAcademicYearRepository academicYearRepository,
    IFeeStructureRepository feeStructureRepository)
    : IRequestHandler<TransferStudentCommand, Result<EnrollmentDto>>
{
    public async Task<Result<EnrollmentDto>> Handle(TransferStudentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new TransferStudentCommandValidator();
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

            // Validate current enrollment exists and is active
            var currentEnrollment = await enrollmentRepository.GetByIdWithDetailsAsync(command.EnrollmentId);
            if (currentEnrollment is null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.NotFound(
                        "Enrollment.NotFound",
                        $"Current enrollment with ID '{command.EnrollmentId}' was not found"
                    )
                );
            }

            if (!currentEnrollment.IsActive)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.NotActive",
                        "Cannot transfer student from an inactive enrollment"
                    ),
                    "Current enrollment is not active"
                );
            }

            // Validate new class exists
            var newClass = await classRepository.GetByIdAsync(command.NewClassId);
            if (newClass is null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.NotFound(
                        "Class.NotFound",
                        $"New class with ID '{command.NewClassId}' was not found"
                    )
                );
            }

            // Validate new fee structure exists
            var newFeeStructure = await feeStructureRepository.GetByIdWithFeeItemsAsync(command.NewFeeStructureId);
            if (newFeeStructure is null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.NotFound(
                        "FeeStructure.NotFound",
                        $"New fee structure with ID '{command.NewFeeStructureId}' was not found"
                    )
                );
            }

            // Validate new class belongs to the same academic year as current enrollment
            if (newClass.AcademicYearId != currentEnrollment.AcademicYearId)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.DifferentAcademicYear",
                        $"New class belongs to a different academic year"
                    ),
                    "Cannot transfer student to a class in a different academic year. Use promotion instead."
                );
            }

            // Validate new fee structure belongs to new class
            if (newFeeStructure.ClassId != command.NewClassId)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.FeeStructureClassMismatch",
                        $"New fee structure does not belong to the specified new class"
                    ),
                    "Selected fee structure does not belong to the specified new class"
                );
            }

            // Check if student is already enrolled in the new class
            var existingEnrollment = await enrollmentRepository.GetActiveEnrollmentByStudentAndClassAsync(
                currentEnrollment.StudentId, command.NewClassId);

            if (existingEnrollment is not null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.AlreadyEnrolled",
                        $"Student is already enrolled in class '{existingEnrollment.Class.Name}'"
                    ),
                    "Student is already enrolled in the target class"
                );
            }

            // Check if there are any outstanding payments in current enrollment
            var currentBalance = currentEnrollment.CalculateBalance();
            if (currentBalance.Amount > 0)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.OutstandingBalance",
                        $"Cannot transfer student with outstanding balance of {currentBalance.Amount} {currentBalance.Currency}"
                    ),
                    "Cannot transfer student while there is an outstanding balance"
                );
            }

            // Create parameters object
            var parameters = new TransferStudentParameters(
                command.EnrollmentId,
                command.NewClassId,
                command.NewFeeStructureId);

            // Call repository with transaction support
            var repositoryResult = await enrollmentRepository.TransferStudentAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Updated)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.NotFound => Result<EnrollmentDto>.Failed(
                        Error.NotFound(
                            "Enrollment.NotFound",
                            $"Current enrollment was not found"
                        ),
                        "Current enrollment not found"
                    ),
                    RepositoryActionStatus.Invalid => Result<EnrollmentDto>.Failed(
                        Error.Validation(
                            "Enrollment.TransferInvalid",
                            $"Student transfer is not allowed"
                        ),
                        "Student transfer is not allowed due to validation rules"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.ConcurrencyConflict",
                            "A concurrency conflict occurred while transferring the student"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.NotTransferred",
                            "Student was not transferred"
                        ),
                        "No changes were made to transfer the student"
                    ),
                    _ => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.TransferFailed",
                            $"An unexpected error occurred while transferring the student. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while transferring the student"
                    )
                };
            }

            // Get related entities for DTO mapping
            var student = await studentRepository.GetByIdAsync(currentEnrollment.StudentId);
            var academicYear = await academicYearRepository.GetByIdAsync(currentEnrollment.AcademicYearId);

            // Manually map to DTO
            var updatedEnrollmentDto = MapToEnrollmentDto(
                repositoryResult.Entity!,
                student!,
                newClass,
                academicYear!,
                newFeeStructure);

            return Result<EnrollmentDto>.Succeeded(
                updatedEnrollmentDto,
                "Student transferred successfully to the new class"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error transferring student from enrollment {EnrollmentId}", command.EnrollmentId);

            return Result<EnrollmentDto>.Failed(
                Error.Failure(
                    "Enrollment.TransferFailed",
                    $"An error occurred while transferring the student: {ex.Message}"
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

public record TransferStudentParameters(
    Guid EnrollmentId,
    Guid NewClassId,
    Guid NewFeeStructureId);

public record TransferStudentRequestDto(
    Guid EnrollmentId,
    Guid NewClassId,
    Guid NewFeeStructureId);
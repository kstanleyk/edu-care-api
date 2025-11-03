using EduCare.Application.Features.Core.Dtos;
using EduCare.Application.Features.Core.EnrollmentManagement.Validators;
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Commands;

public record PromoteStudentCommand(
    Guid EnrollmentId,
    Guid NextClassId,
    Guid NextAcademicYearId,
    Guid NewFeeStructureId) : IRequest<Result<EnrollmentDto>>;

public class PromoteStudentCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    IStudentRepository studentRepository,
    IClassRepository classRepository,
    IAcademicYearRepository academicYearRepository,
    IFeeStructureRepository feeStructureRepository)
    : IRequestHandler<PromoteStudentCommand, Result<EnrollmentDto>>
{
    public async Task<Result<EnrollmentDto>> Handle(PromoteStudentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new PromoteStudentCommandValidator();
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
                        "Cannot promote student from an inactive enrollment"
                    ),
                    "Current enrollment is not active"
                );
            }

            // Validate next class exists
            var nextClass = await classRepository.GetByIdAsync(command.NextClassId);
            if (nextClass is null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.NotFound(
                        "Class.NotFound",
                        $"Next class with ID '{command.NextClassId}' was not found"
                    )
                );
            }

            // Validate next academic year exists
            var nextAcademicYear = await academicYearRepository.GetByIdAsync(command.NextAcademicYearId);
            if (nextAcademicYear is null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.NotFound(
                        "AcademicYear.NotFound",
                        $"Next academic year with ID '{command.NextAcademicYearId}' was not found"
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

            // Validate next class belongs to next academic year
            if (nextClass.AcademicYearId != command.NextAcademicYearId)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.ClassAcademicYearMismatch",
                        $"Next class does not belong to the specified next academic year"
                    ),
                    "Selected next class does not belong to the specified next academic year"
                );
            }

            // Validate new fee structure belongs to next class
            if (newFeeStructure.ClassId != command.NextClassId)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.FeeStructureClassMismatch",
                        $"New fee structure does not belong to the specified next class"
                    ),
                    "Selected fee structure does not belong to the specified next class"
                );
            }

            // Check if student is already enrolled in the next academic year
            var existingEnrollment = await enrollmentRepository.GetActiveEnrollmentByStudentAndAcademicYearAsync(
                currentEnrollment.StudentId, command.NextAcademicYearId);

            if (existingEnrollment is not null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.AlreadyEnrolled",
                        $"Student is already enrolled in academic year '{existingEnrollment.AcademicYear.Name}'"
                    ),
                    "Student is already enrolled in the next academic year"
                );
            }

            // Check if there are any outstanding payments in current enrollment
            var currentBalance = currentEnrollment.CalculateBalance();
            if (currentBalance.Amount > 0)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.OutstandingBalance",
                        $"Cannot promote student with outstanding balance of {currentBalance.Amount} {currentBalance.Currency} in current enrollment"
                    ),
                    "Cannot promote student while there is an outstanding balance in the current enrollment"
                );
            }

            // Create parameters object
            var parameters = new PromoteStudentParameters(
                command.EnrollmentId,
                command.NextClassId,
                command.NextAcademicYearId,
                command.NewFeeStructureId);

            // Call repository with transaction support
            var repositoryResult = await enrollmentRepository.PromoteStudentAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Created)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.Invalid => Result<EnrollmentDto>.Failed(
                        Error.Validation(
                            "Enrollment.AlreadyEnrolled",
                            $"Student is already enrolled in the next academic year"
                        ),
                        "Student is already enrolled in the next academic year"
                    ),
                    RepositoryActionStatus.NotFound => Result<EnrollmentDto>.Failed(
                        Error.NotFound(
                            "Enrollment.NotFound",
                            $"One or more related entities were not found"
                        ),
                        "One or more related entities were not found"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.ConcurrencyConflict",
                            "A concurrency conflict occurred while promoting the student"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.NotPromoted",
                            "Student was not promoted"
                        ),
                        "No changes were made to promote the student"
                    ),
                    _ => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.PromotionFailed",
                            $"An unexpected error occurred while promoting the student. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while promoting the student"
                    )
                };
            }

            // Get related entities for DTO mapping
            var student = await studentRepository.GetByIdAsync(currentEnrollment.StudentId);

            // Manually map to DTO
            var newEnrollmentDto = MapToEnrollmentDto(
                repositoryResult.Entity!,
                student!,
                nextClass,
                nextAcademicYear,
                newFeeStructure);

            return Result<EnrollmentDto>.Succeeded(
                newEnrollmentDto,
                "Student promoted successfully to the next class"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error promoting student from enrollment {EnrollmentId}", command.EnrollmentId);

            return Result<EnrollmentDto>.Failed(
                Error.Failure(
                    "Enrollment.PromotionFailed",
                    $"An error occurred while promoting the student: {ex.Message}"
                ),
                "An unexpected error occurred while processing your request"
            );
        }
    }

    public record PromoteStudentParameters(
        Guid EnrollmentId,
        Guid NextClassId,
        Guid NextAcademicYearId,
        Guid NewFeeStructureId);

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

public record PromoteStudentRequestDto(
    Guid EnrollmentId,
    Guid NextClassId,
    Guid NextAcademicYearId,
    Guid NewFeeStructureId);
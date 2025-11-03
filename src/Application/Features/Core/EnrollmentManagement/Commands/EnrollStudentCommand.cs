using EduCare.Application.Features.Core.Dtos;
using EduCare.Application.Features.Core.EnrollmentManagement.Validators;
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Commands;

public record EnrollStudentCommand(
    Guid StudentId,
    Guid ClassId,
    Guid AcademicYearId,
    Guid FeeStructureId,
    DateOnly EnrollmentDate) : IRequest<Result<EnrollmentDto>>;

public class EnrollStudentCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    IStudentRepository studentRepository,
    IClassRepository classRepository,
    IAcademicYearRepository academicYearRepository,
    IFeeStructureRepository feeStructureRepository)
    : IRequestHandler<EnrollStudentCommand, Result<EnrollmentDto>>
{
    public async Task<Result<EnrollmentDto>> Handle(EnrollStudentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new EnrollStudentCommandValidator();
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

            // Validate student exists
            var student = await studentRepository.GetByIdAsync(command.StudentId);
            if (student is null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.NotFound(
                        "Student.NotFound",
                        $"Student with ID '{command.StudentId}' was not found"
                    )
                );
            }

            // Validate class exists
            var classEntity = await classRepository.GetByIdAsync(command.ClassId);
            if (classEntity is null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.NotFound(
                        "Class.NotFound",
                        $"Class with ID '{command.ClassId}' was not found"
                    )
                );
            }

            // Validate academic year exists
            var academicYear = await academicYearRepository.GetByIdAsync(command.AcademicYearId);
            if (academicYear is null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.NotFound(
                        "AcademicYear.NotFound",
                        $"Academic year with ID '{command.AcademicYearId}' was not found"
                    )
                );
            }

            // Validate fee structure exists
            var feeStructure = await feeStructureRepository.GetByIdWithFeeItemsAsync(command.FeeStructureId);
            if (feeStructure is null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.NotFound(
                        "FeeStructure.NotFound",
                        $"Fee structure with ID '{command.FeeStructureId}' was not found"
                    )
                );
            }

            // Validate class belongs to academic year
            if (classEntity.AcademicYearId != command.AcademicYearId)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.ClassAcademicYearMismatch",
                        $"Class does not belong to the specified academic year"
                    ),
                    "Selected class does not belong to the specified academic year"
                );
            }

            // Validate fee structure belongs to class
            if (feeStructure.ClassId != command.ClassId)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.FeeStructureClassMismatch",
                        $"Fee structure does not belong to the specified class"
                    ),
                    "Selected fee structure does not belong to the specified class"
                );
            }

            // Check if student is already enrolled in the same academic year
            var existingEnrollment = await enrollmentRepository.GetActiveEnrollmentByStudentAndAcademicYearAsync(
                command.StudentId, command.AcademicYearId);

            if (existingEnrollment is not null)
            {
                return Result<EnrollmentDto>.Failed(
                    Error.Validation(
                        "Enrollment.AlreadyEnrolled",
                        $"Student is already enrolled in academic year '{existingEnrollment.AcademicYear.Name}'"
                    ),
                    "Student is already enrolled in this academic year"
                );
            }

            // Create parameters object
            var parameters = new EnrollStudentParameters(
                command.StudentId,
                command.ClassId,
                command.AcademicYearId,
                command.FeeStructureId,
                command.EnrollmentDate);

            // Call repository with transaction support
            var repositoryResult = await enrollmentRepository.EnrollStudentAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Created)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.Invalid => Result<EnrollmentDto>.Failed(
                        Error.Validation(
                            "Enrollment.AlreadyEnrolled",
                            $"Student is already enrolled in this academic year"
                        ),
                        "Student is already enrolled in this academic year"
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
                            "A concurrency conflict occurred while enrolling the student"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.NotCreated",
                            "Enrollment was not created"
                        ),
                        "No changes were made to create the enrollment"
                    ),
                    _ => Result<EnrollmentDto>.Failed(
                        Error.Failure(
                            "Enrollment.CreationFailed",
                            $"An unexpected error occurred while enrolling the student. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while enrolling the student"
                    )
                };
            }

            // Manually map to DTO
            var enrollmentDto = MapToEnrollmentDto(repositoryResult.Entity!, student, classEntity, academicYear, feeStructure);

            return Result<EnrollmentDto>.Succeeded(
                enrollmentDto,
                "Student enrolled successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error enrolling student {StudentId} in class {ClassId}", command.StudentId, command.ClassId);

            return Result<EnrollmentDto>.Failed(
                Error.Failure(
                    "Enrollment.CreationFailed",
                    $"An error occurred while enrolling the student: {ex.Message}"
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

public record EnrollStudentRequestDto(
    Guid StudentId,
    Guid ClassId,
    Guid AcademicYearId,
    Guid FeeStructureId,
    DateOnly EnrollmentDate);

public record EnrollStudentParameters(
    Guid StudentId,
    Guid ClassId,
    Guid AcademicYearId,
    Guid FeeStructureId,
    DateOnly EnrollmentDate);
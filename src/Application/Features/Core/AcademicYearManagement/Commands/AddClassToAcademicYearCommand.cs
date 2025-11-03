using EduCare.Application.Features.Core.AcademicYearManagement.Validators;
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.AcademicYearManagement.Commands;

public record AddClassToAcademicYearCommand(Guid AcademicYearId, string Name, string Code, int GradeLevel)
    : IRequest<Result<ClassDto>>;

public class AddClassToAcademicYearCommandHandler(
    IAcademicYearRepository academicYearRepository,
    IClassRepository classRepository)
    : IRequestHandler<AddClassToAcademicYearCommand, Result<ClassDto>>
{
    public async Task<Result<ClassDto>> Handle(AddClassToAcademicYearCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new AddClassToAcademicYearCommandValidator();
            var validationResult = await validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList();
                return Result<ClassDto>.Failed(
                    Error.Validation("Class.ValidationError", string.Join(", ", validationErrors))
                );
            }

            // Validate academic year exists
            var academicYear = await academicYearRepository.GetByIdWithClassesAsync(command.AcademicYearId);
            if (academicYear is null)
            {
                return Result<ClassDto>.Failed(
                    Error.NotFound(
                        "AcademicYear.NotFound",
                        $"Academic year with ID '{command.AcademicYearId}' was not found"
                    )
                );
            }

            // Check for duplicate class code in the same academic year
            var existingClass = await classRepository.GetByCodeAndAcademicYearIdAsync(command.Code, command.AcademicYearId);
            if (existingClass is not null)
            {
                return Result<ClassDto>.Failed(
                    Error.Validation(
                        "Class.DuplicateCode",
                        $"Class with code '{command.Code}' already exists in this academic year"
                    )
                );
            }

            // Create class using domain factory method
            var classEntity = Class.Create(command.Name, command.Code, command.GradeLevel, command.AcademicYearId);

            // Call repository with transaction support
            var repositoryResult = await classRepository.CreateClassAsync(classEntity);

            if (repositoryResult.Status != RepositoryActionStatus.Created)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.Invalid => Result<ClassDto>.Failed(
                        Error.Validation(
                            "Class.DuplicateCode",
                            $"Class with code '{command.Code}' already exists in this academic year"
                        ),
                        "Class creation failed due to duplicate code"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<ClassDto>.Failed(
                        Error.Failure(
                            "Class.ConcurrencyConflict",
                            "A concurrency conflict occurred while creating the class"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<ClassDto>.Failed(
                        Error.Failure(
                            "Class.NotCreated",
                            "Class was not created"
                        ),
                        "No changes were made to create the class"
                    ),
                    _ => Result<ClassDto>.Failed(
                        Error.Failure(
                            "Class.CreationFailed",
                            $"An unexpected error occurred while creating the class. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while creating the class"
                    )
                };
            }

            // Manually map to DTO
            var classDto = MapToClassDto(repositoryResult.Entity!, academicYear.Name, academicYear.Code);

            return Result<ClassDto>.Succeeded(
                classDto,
                "Class created successfully and added to academic year"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error creating class for academic year {AcademicYearId}", command.AcademicYearId);

            return Result<ClassDto>.Failed(
                Error.Failure(
                    "Class.CreationFailed",
                    $"An error occurred while creating the class: {ex.Message}"
                ),
                "An unexpected error occurred while processing your request"
            );
        }
    }

    private static ClassDto MapToClassDto(Class classEntity, string academicYearName, string academicYearCode)
    {
        return new ClassDto(
            Id: classEntity.Id,
            Name: classEntity.Name,
            Code: classEntity.Code,
            GradeLevel: classEntity.GradeLevel,
            AcademicYearId: classEntity.AcademicYearId,
            AcademicYearName: academicYearName,
            AcademicYearCode: academicYearCode,
            CreatedOn: classEntity.CreatedOn,
            ModifiedOn: classEntity.ModifiedOn
        );
    }
}
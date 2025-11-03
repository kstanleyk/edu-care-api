using EduCare.Application.Features.Core.AcademicYearManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.AcademicYearManagement.Commands;

public record CreateAcademicYearCommand(
    string Name,
    string Code,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid SchoolId,
    bool IsCurrent = false) : IRequest<Result<AcademicYearDto>>;

public class CreateAcademicYearCommandHandler(
    IAcademicYearRepository academicYearRepository,
    ISchoolRepository schoolRepository)
    : IRequestHandler<CreateAcademicYearCommand, Result<AcademicYearDto>>
{
    public async Task<Result<AcademicYearDto>> Handle(CreateAcademicYearCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new CreateAcademicYearCommandValidator();
            var validationResult = await validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList();
                return Result<AcademicYearDto>.Failed(
                    Error.Validation("AcademicYear.ValidationError", string.Join(", ", validationErrors))
                );
            }

            // Validate school exists
            var school = await schoolRepository.GetByIdAsync(command.SchoolId);
            if (school is null)
            {
                return Result<AcademicYearDto>.Failed(
                    Error.NotFound(
                        "School.NotFound",
                        $"School with ID '{command.SchoolId}' was not found"
                    )
                );
            }

            // Create academic year using domain factory method
            var academicYear = AcademicYear.Create(command.Name, command.Code, command.StartDate, command.EndDate,
                command.SchoolId, command.IsCurrent);

            // Call repository with transaction support
            var repositoryResult = await academicYearRepository.CreateAcademicYearAsync(academicYear);

            if (repositoryResult.Status != RepositoryActionStatus.Created)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.Invalid => Result<AcademicYearDto>.Failed(
                        Error.Validation(
                            "AcademicYear.DuplicateCode",
                            $"Academic year with code '{command.Code}' already exists for this school"
                        ),
                        "Academic year creation failed due to duplicate code"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<AcademicYearDto>.Failed(
                        Error.Failure(
                            "AcademicYear.ConcurrencyConflict",
                            "A concurrency conflict occurred while creating the academic year"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<AcademicYearDto>.Failed(
                        Error.Failure(
                            "AcademicYear.NotCreated",
                            "Academic year was not created"
                        ),
                        "No changes were made to the academic year"
                    ),
                    _ => Result<AcademicYearDto>.Failed(
                        Error.Failure(
                            "AcademicYear.CreationFailed",
                            $"An unexpected error occurred while creating the academic year. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while creating the academic year"
                    )
                };
            }

            // Manually map to DTO
            var academicYearDto = MapToAcademicYearDto(repositoryResult.Entity!, school.Name);

            return Result<AcademicYearDto>.Succeeded(
                academicYearDto,
                "Academic year created successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error creating academic year for school {SchoolId}", command.SchoolId);

            return Result<AcademicYearDto>.Failed(
                Error.Failure(
                    "AcademicYear.CreationFailed",
                    $"An error occurred while creating the academic year: {ex.Message}"
                ),
                "An unexpected error occurred while processing your request"
            );
        }
    }

    private static AcademicYearDto MapToAcademicYearDto(AcademicYear academicYear, string schoolName)
    {
        return new AcademicYearDto(
            Id: academicYear.Id,
            Name: academicYear.Name,
            Code: academicYear.Code,
            StartDate: academicYear.StartDate,
            EndDate: academicYear.EndDate,
            IsCurrent: academicYear.IsCurrent,
            SchoolId: academicYear.SchoolId,
            SchoolName: schoolName,
            CreatedOn: academicYear.CreatedOn,
            ModifiedOn: academicYear.ModifiedOn,
            Classes: academicYear.Classes.Select(MapToClassSummaryDto).ToList()
        );
    }

    private static ClassSummaryDto MapToClassSummaryDto(Class @class)
    {
        return new ClassSummaryDto(
            Id: @class.Id,
            Name: @class.Name,
            Code: @class.Code,
            GradeLevel: @class.GradeLevel
        );
    }
}

public record CreateAcademicYearRequestDto(
    string Name,
    string Code,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid SchoolId,
    bool IsCurrent = false);

public record AcademicYearDto(
    Guid Id,
    string Name,
    string Code,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent,
    Guid SchoolId,
    string SchoolName,
    DateTime CreatedOn,
    DateTime? ModifiedOn,
    IReadOnlyCollection<ClassSummaryDto> Classes);

public record ClassSummaryDto(
    Guid Id,
    string Name,
    string Code,
    int GradeLevel);
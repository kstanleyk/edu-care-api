using EduCare.Application.Features.Core.AcademicYearManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.AcademicYearManagement.Commands;

public record MarkAcademicYearAsCurrentCommand(Guid Id) : IRequest<Result<AcademicYearDto>>;

public class MarkAcademicYearAsCurrentCommandHandler(
    IAcademicYearRepository academicYearRepository,
    ISchoolRepository schoolRepository)
    : IRequestHandler<MarkAcademicYearAsCurrentCommand, Result<AcademicYearDto>>
{
    public async Task<Result<AcademicYearDto>> Handle(MarkAcademicYearAsCurrentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new MarkAcademicYearAsCurrentCommandValidator();
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

            // Validate academic year exists
            var academicYear = await academicYearRepository.GetByIdWithClassesAsync(command.Id);
            if (academicYear is null)
            {
                return Result<AcademicYearDto>.Failed(
                    Error.NotFound(
                        "AcademicYear.NotFound",
                        $"Academic year with ID '{command.Id}' was not found"
                    )
                );
            }

            // Get school name separately
            var school = await schoolRepository.GetByIdAsync(academicYear.SchoolId);
            if (school is null)
            {
                return Result<AcademicYearDto>.Failed(
                    Error.NotFound(
                        "School.NotFound",
                        $"School with ID '{academicYear.SchoolId}' was not found"
                    )
                );
            }

            // Check if already current
            if (academicYear.IsCurrent)
            {
                return Result<AcademicYearDto>.Failed(
                    Error.Validation(
                        "AcademicYear.AlreadyCurrent",
                        "Academic year is already marked as current"
                    ),
                    "Academic year is already set as current"
                );
            }

            // Call repository with transaction support
            var repositoryResult = await academicYearRepository.MarkAsCurrentAsync(command.Id);

            if (repositoryResult.Status != RepositoryActionStatus.Updated)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.NotFound => Result<AcademicYearDto>.Failed(
                        Error.NotFound(
                            "AcademicYear.NotFound",
                            $"Academic year with ID '{command.Id}' was not found"
                        ),
                        "Academic year not found"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<AcademicYearDto>.Failed(
                        Error.Failure(
                            "AcademicYear.ConcurrencyConflict",
                            "A concurrency conflict occurred while updating the academic year"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<AcademicYearDto>.Failed(
                        Error.Failure(
                            "AcademicYear.NotUpdated",
                            "Academic year was not updated"
                        ),
                        "No changes were made to the academic year"
                    ),
                    _ => Result<AcademicYearDto>.Failed(
                        Error.Failure(
                            "AcademicYear.UpdateFailed",
                            $"An unexpected error occurred while updating the academic year. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while updating the academic year"
                    )
                };
            }

            // Manually map to DTO
            var academicYearDto = MapToAcademicYearDto(repositoryResult.Entity!, school.Name);

            return Result<AcademicYearDto>.Succeeded(
                academicYearDto,
                "Academic year successfully marked as current"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error marking academic year {AcademicYearId} as current", command.Id);

            return Result<AcademicYearDto>.Failed(
                Error.Failure(
                    "AcademicYear.UpdateFailed",
                    $"An error occurred while marking the academic year as current: {ex.Message}"
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
using EduCare.Application.Features.Core.AcademicYearManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.AcademicYearManagement.Commands;

public record UpdateAcademicYearCommand(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent) : IRequest<Result<AcademicYearDto>>;

public class UpdateAcademicYearCommandHandler(
    IAcademicYearRepository academicYearRepository,
    ISchoolRepository schoolRepository)
    : IRequestHandler<UpdateAcademicYearCommand, Result<AcademicYearDto>>
{
    public async Task<Result<AcademicYearDto>> Handle(UpdateAcademicYearCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new UpdateAcademicYearCommandValidator();
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

            // Create parameters object
            var parameters = new UpdateAcademicYearParameters(
                command.Id,
                command.Name,
                command.StartDate,
                command.EndDate,
                command.IsCurrent);

            // Call repository with parameters object
            var repositoryResult = await academicYearRepository.UpdateAcademicYearAsync(parameters);

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
                "Academic year updated successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error updating academic year {AcademicYearId}", command.Id);

            return Result<AcademicYearDto>.Failed(
                Error.Failure(
                    "AcademicYear.UpdateFailed",
                    $"An error occurred while updating the academic year: {ex.Message}"
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

public record UpdateAcademicYearRequestDto(
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent);

public record UpdateAcademicYearParameters(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent);
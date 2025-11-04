using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Features.Core.OrganizationManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.OrganizationManagement.Commands;

// Command
public record AddSchoolToOrganizationCommand(Guid OrganizationId, Guid SchoolId)
    : IRequest<Result<OrganizationDto>>;

public class AddSchoolToOrganizationCommandHandler(
    IOrganizationRepository organizationRepository,
    ISchoolRepository schoolRepository)
    : IRequestHandler<AddSchoolToOrganizationCommand, Result<OrganizationDto>>
{
    public async Task<Result<OrganizationDto>> Handle(AddSchoolToOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new AddSchoolToOrganizationCommandValidator();
            var validationResult = await validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList();
                return Result<OrganizationDto>.Failed(
                    Error.Validation("Organization.ValidationError", string.Join(", ", validationErrors))
                );
            }

            // Validate organization exists
            var organization = await organizationRepository.GetByIdWithSchoolsAsync(command.OrganizationId);
            if (organization is null)
            {
                return Result<OrganizationDto>.Failed(
                    Error.NotFound(
                        "Organization.NotFound",
                        $"Organization with ID '{command.OrganizationId}' was not found"
                    )
                );
            }

            // Validate school exists
            var school = await schoolRepository.GetByIdAsync(command.SchoolId);
            if (school is null)
            {
                return Result<OrganizationDto>.Failed(
                    Error.NotFound(
                        "School.NotFound",
                        $"School with ID '{command.SchoolId}' was not found"
                    )
                );
            }

            // Check if school is already part of the organization
            if (organization.Schools.Any(s => s.Id == command.SchoolId))
            {
                return Result<OrganizationDto>.Failed(
                    Error.Validation(
                        "Organization.SchoolAlreadyAdded",
                        $"School '{school.Name}' is already part of this organization"
                    ),
                    "School is already part of this organization"
                );
            }

            // Check if school already belongs to another organization
            if (school.OrganizationId != Guid.Empty && school.OrganizationId != command.OrganizationId)
            {
                return Result<OrganizationDto>.Failed(
                    Error.Validation(
                        "School.AlreadyInOrganization",
                        $"School '{school.Name}' already belongs to another organization"
                    ),
                    "School already belongs to another organization"
                );
            }

            // Create parameters object
            var parameters = new AddSchoolToOrganizationParameters(
                command.OrganizationId,
                command.SchoolId);

            // Call repository with transaction support
            var repositoryResult = await organizationRepository.AddSchoolToOrganizationAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Updated)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.NotFound => Result<OrganizationDto>.Failed(
                        Error.NotFound(
                            "Organization.NotFound",
                            $"Organization or school was not found"
                        ),
                        "Organization or school not found"
                    ),
                    RepositoryActionStatus.Invalid => Result<OrganizationDto>.Failed(
                        Error.Validation(
                            "Organization.SchoolAlreadyAdded",
                            $"School is already part of this organization"
                        ),
                        "School is already part of this organization"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<OrganizationDto>.Failed(
                        Error.Failure(
                            "Organization.ConcurrencyConflict",
                            "A concurrency conflict occurred while adding the school to the organization"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<OrganizationDto>.Failed(
                        Error.Failure(
                            "Organization.NotUpdated",
                            "Organization was not updated"
                        ),
                        "No changes were made to the organization"
                    ),
                    _ => Result<OrganizationDto>.Failed(
                        Error.Failure(
                            "Organization.UpdateFailed",
                            $"An unexpected error occurred while adding the school to the organization. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while updating the organization"
                    )
                };
            }

            // Manually map to DTO
            var organizationDto = MapToOrganizationDto(repositoryResult.Entity!);

            return Result<OrganizationDto>.Succeeded(
                organizationDto,
                "School added to organization successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error adding school {SchoolId} to organization {OrganizationId}", command.SchoolId, command.OrganizationId);

            return Result<OrganizationDto>.Failed(
                Error.Failure(
                    "Organization.UpdateFailed",
                    $"An error occurred while adding the school to the organization: {ex.Message}"
                ),
                "An unexpected error occurred while processing your request"
            );
        }
    }

    private static OrganizationDto MapToOrganizationDto(Organization organization)
    {
        return new OrganizationDto(
            Id: organization.Id,
            Name: organization.Name,
            Code: organization.Code,
            Address: organization.Address,
            CreatedOn: organization.CreatedOn,
            ModifiedOn: organization.ModifiedOn,
            Schools: organization.Schools.Select(MapToSchoolDto).ToList()
        );
    }

    private static SchoolDto MapToSchoolDto(School school)
    {
        return new SchoolDto(
            Id: school.Id,
            Name: school.Name,
            Code: school.Code,
            Type: school.Type,
            Mode: school.Mode,
            Address: school.Address,
            OrganizationId: school.OrganizationId,
            CreatedOn: school.CreatedOn,
            ModifiedOn: school.ModifiedOn
        );
    }
}

// Request DTO (if needed for API)
public record AddSchoolToOrganizationRequestDto(Guid OrganizationId, Guid SchoolId);

// Parameters for repository
public record AddSchoolToOrganizationParameters(Guid OrganizationId, Guid SchoolId);
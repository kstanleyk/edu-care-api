using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Features.Core.OrganizationManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.OrganizationManagement.Commands;

// Command
public record UpdateOrganizationCommand(Guid Id, string Name, Address? Address)
    : IRequest<Result<OrganizationDto>>;

public class UpdateOrganizationCommandHandler(
    IOrganizationRepository organizationRepository)
    : IRequestHandler<UpdateOrganizationCommand, Result<OrganizationDto>>
{
    public async Task<Result<OrganizationDto>> Handle(UpdateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new UpdateOrganizationCommandValidator();
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
            var organization = await organizationRepository.GetByIdWithSchoolsAsync(command.Id);
            if (organization is null)
            {
                return Result<OrganizationDto>.Failed(
                    Error.NotFound(
                        "Organization.NotFound",
                        $"Organization with ID '{command.Id}' was not found"
                    )
                );
            }

            // Check if there are any actual changes
            if (!HasChanges(organization, command))
            {
                return Result<OrganizationDto>.Failed(
                    Error.Validation(
                        "Organization.NoChanges",
                        "No changes were made to the organization"
                    ),
                    "No changes were made to the organization"
                );
            }

            // Create parameters object
            var parameters = new UpdateOrganizationParameters(
                command.Id,
                command.Name,
                command.Address);

            // Call repository with transaction support
            var repositoryResult = await organizationRepository.UpdateOrganizationAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Updated)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.NotFound => Result<OrganizationDto>.Failed(
                        Error.NotFound(
                            "Organization.NotFound",
                            $"Organization with ID '{command.Id}' was not found"
                        ),
                        "Organization not found"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<OrganizationDto>.Failed(
                        Error.Failure(
                            "Organization.ConcurrencyConflict",
                            "A concurrency conflict occurred while updating the organization"
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
                            $"An unexpected error occurred while updating the organization. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while updating the organization"
                    )
                };
            }

            // Manually map to DTO
            var organizationDto = MapToOrganizationDto(repositoryResult.Entity!);

            return Result<OrganizationDto>.Succeeded(
                organizationDto,
                "Organization updated successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error updating organization {OrganizationId}", command.Id);

            return Result<OrganizationDto>.Failed(
                Error.Failure(
                    "Organization.UpdateFailed",
                    $"An error occurred while updating the organization: {ex.Message}"
                ),
                "An unexpected error occurred while processing your request"
            );
        }
    }

    /// <summary>
    /// Checks if there are any actual changes between the current organization and the command
    /// </summary>
    private static bool HasChanges(Organization organization, UpdateOrganizationCommand command)
    {
        return organization.Name != command.Name ||
               !Address.AddressEquals(organization.Address, command.Address);
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
public record UpdateOrganizationRequestDto(Guid Id, string Name, Address? Address);

// Parameters for repository
public record UpdateOrganizationParameters(Guid Id, string Name, Address? Address);
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Features.Core.OrganizationManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.OrganizationManagement.Commands;

public record CreateOrganizationCommand(string Name, string Code, Address? Address)
    : IRequest<Result<OrganizationDto>>;

public class CreateOrganizationCommandHandler(
    IOrganizationRepository organizationRepository)
    : IRequestHandler<CreateOrganizationCommand, Result<OrganizationDto>>
{
    public async Task<Result<OrganizationDto>> Handle(CreateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new CreateOrganizationCommandValidator();
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

            // Check if organization with same code already exists
            var existingOrganization = await organizationRepository.GetByCodeAsync(command.Code);
            if (existingOrganization is not null)
            {
                return Result<OrganizationDto>.Failed(
                    Error.Validation(
                        "Organization.DuplicateCode",
                        $"Organization with code '{command.Code}' already exists"
                    ),
                    "An organization with this code already exists"
                );
            }

            // Create parameters object
            var parameters = new CreateOrganizationParameters(
                command.Name,
                command.Code,
                command.Address);

            // Call repository with transaction support
            var repositoryResult = await organizationRepository.CreateOrganizationAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Created)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.Conflict => Result<OrganizationDto>.Failed(
                        Error.Conflict(
                            "Organization.Conflict",
                            "Organization creation failed due to conflict"
                        ),
                        "Organization creation failed due to conflict"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<OrganizationDto>.Failed(
                        Error.Failure(
                            "Organization.ConcurrencyConflict",
                            "A concurrency conflict occurred while creating the organization"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<OrganizationDto>.Failed(
                        Error.Failure(
                            "Organization.NotCreated",
                            "Organization was not created"
                        ),
                        "No changes were made to create the organization"
                    ),
                    _ => Result<OrganizationDto>.Failed(
                        Error.Failure(
                            "Organization.CreationFailed",
                            $"An unexpected error occurred while creating the organization. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while creating the organization"
                    )
                };
            }

            // Manually map to DTO
            var organizationDto = MapToOrganizationDto(repositoryResult.Entity!);

            return Result<OrganizationDto>.Succeeded(
                organizationDto,
                "Organization created successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error creating organization with code {Code}", command.Code);

            return Result<OrganizationDto>.Failed(
                Error.Failure(
                    "Organization.CreationFailed",
                    $"An error occurred while creating the organization: {ex.Message}"
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
public record CreateOrganizationRequestDto(string Name, string Code, Address? Address);

// Parameters for repository
public record CreateOrganizationParameters(string Name, string Code, Address? Address);
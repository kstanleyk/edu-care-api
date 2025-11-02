using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.OrganizationManagement.Queries;

public record GetOrganizationSchoolsQuery(Guid OrganizationId) : IRequest<Result<List<SchoolDto>>>;

public class GetOrganizationSchoolsQueryHandler(
    IOrganizationRepository organizationRepository,
    ISchoolRepository schoolRepository)
    : IRequestHandler<GetOrganizationSchoolsQuery, Result<List<SchoolDto>>>
{
    public async Task<Result<List<SchoolDto>>> Handle(GetOrganizationSchoolsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate organization exists
            var organization = await organizationRepository.GetByIdAsync(query.OrganizationId);
            if (organization is null)
            {
                return Result<List<SchoolDto>>.Failed(
                    Error.NotFound(
                        "Organization.NotFound",
                        $"Organization with ID {query.OrganizationId} was not found"
                    )
                );
            }

            // Get all schools for the organization
            var schools = await schoolRepository.GetByOrganizationIdAsync(query.OrganizationId);

            // Manually map School entities to SchoolDto list without AutoMapper
            var schoolDtos = schools.Select(MapToSchoolDto).ToList();

            return Result<List<SchoolDto>>.Succeeded(schoolDtos);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting schools for organization {OrganizationId}", query.OrganizationId);

            return Result<List<SchoolDto>>.Failed(
                Error.Failure(
                    "OrganizationSchoolsQuery.Failed",
                    $"An error occurred while retrieving the organization schools: {ex.Message}"
                )
            );
        }
    }

    private static SchoolDto MapToSchoolDto(School school)
    {
        return new SchoolDto(
            Id: school.Id,
            Name: school.Name,
            Code: school.Code,
            Type: school.Type,
            Mode: school.Mode,
            OrganizationId: school.OrganizationId,
            Address: school.Address,
            CreatedOn: school.CreatedOn,
            ModifiedOn: school.ModifiedOn
        );
    }
}
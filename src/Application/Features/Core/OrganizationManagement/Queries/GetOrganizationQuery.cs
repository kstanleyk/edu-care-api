using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.OrganizationManagement.Queries;

//public record GetOrganizationQuery(Guid Id) : IRequest<Result<OrganizationDto>>;

//public class GetOrganizationQueryHandler(
//    IOrganizationRepository organizationRepository)
//    : IRequestHandler<GetOrganizationQuery, Result<OrganizationDto>>
//{
//    public async Task<Result<OrganizationDto>> Handle(GetOrganizationQuery query, CancellationToken cancellationToken)
//    {
//        try
//        {
//            // Get organization by ID
//            var organization = await organizationRepository.GetByIdWithDetailsAsync(query.Id);
//            if (organization is null)
//            {
//                return Result<OrganizationDto>.Failed(
//                    Error.NotFound(
//                        "Organization.NotFound",
//                        $"Organization with ID {query.Id} was not found"
//                    )
//                );
//            }

//            // Manually map Organization to OrganizationDto without AutoMapper
//            var organizationDto = MapToOrganizationDto(organization);

//            return Result<OrganizationDto>.Succeeded(organizationDto);
//        }
//        catch (Exception ex)
//        {
//            // Log the exception (in a real application, you'd inject ILogger)
//            // _logger.LogError(ex, "Error getting organization {OrganizationId}", query.Id);

//            return Result<OrganizationDto>.Failed(
//                Error.Failure(
//                    "OrganizationQuery.Failed",
//                    $"An error occurred while retrieving the organization: {ex.Message}"
//                )
//            );
//        }
//    }

//    private static OrganizationDto MapToOrganizationDto(Organization organization)
//    {
//        return new OrganizationDto(
//            Id: organization.Id,
//            Name: organization.Name,
//            Code: organization.Code,
//            Address: organization.Address,
//            CreatedOn: organization.CreatedOn,
//            ModifiedOn: organization.ModifiedOn,
//            Schools: organization.Schools.Select(MapToSchoolSummaryDto).ToList()
//        );
//    }

//    private static SchoolSummaryDto MapToSchoolSummaryDto(School school)
//    {
//        return new SchoolSummaryDto(
//            Id: school.Id,
//            Name: school.Name,
//            Code: school.Code,
//            Type: school.Type,
//            Mode: school.Mode,
//            Address: school.Address,
//            CreatedOn: school.CreatedOn
//        );
//    }
//}
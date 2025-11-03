using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.OrganizationManagement.Queries;

public record GetSchoolQuery(Guid Id) : IRequest<Result<SchoolDto>>;

public class GetSchoolQueryHandler(
    ISchoolRepository schoolRepository)
    : IRequestHandler<GetSchoolQuery, Result<SchoolDto>>
{
    public async Task<Result<SchoolDto>> Handle(GetSchoolQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Get school by ID
            var school = await schoolRepository.GetByIdAsync(query.Id);
            if (school is null)
            {
                return Result<SchoolDto>.Failed(
                    Error.NotFound(
                        "School.NotFound",
                        $"School with ID {query.Id} was not found"
                    )
                );
            }

            // Manually map School to SchoolDto without AutoMapper
            var schoolDto = MapToSchoolDto(school);

            return Result<SchoolDto>.Succeeded(schoolDto);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting school {SchoolId}", query.Id);

            return Result<SchoolDto>.Failed(
                Error.Failure(
                    "SchoolQuery.Failed",
                    $"An error occurred while retrieving the school: {ex.Message}"
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
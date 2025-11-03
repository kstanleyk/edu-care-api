using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.OrganizationManagement.Queries;

public record GetSchoolAcademicYearsQuery(Guid SchoolId) : IRequest<Result<List<AcademicYearDto>>>;

public class GetSchoolAcademicYearsQueryHandler(
    ISchoolRepository schoolRepository,
    IAcademicYearRepository academicYearRepository)
    : IRequestHandler<GetSchoolAcademicYearsQuery, Result<List<AcademicYearDto>>>
{
    public async Task<Result<List<AcademicYearDto>>> Handle(GetSchoolAcademicYearsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate school exists
            var school = await schoolRepository.GetByIdAsync(query.SchoolId);
            if (school is null)
            {
                return Result<List<AcademicYearDto>>.Failed(
                    Error.NotFound(
                        "School.NotFound",
                        $"School with ID {query.SchoolId} was not found"
                    )
                );
            }

            // Get all academic years for the school
            var academicYears = await academicYearRepository.GetBySchoolIdAsync(query.SchoolId);

            // Manually map AcademicYear entities to AcademicYearDto list without AutoMapper
            var academicYearDtos = academicYears.Select(MapToAcademicYearDto).ToList();

            return Result<List<AcademicYearDto>>.Succeeded(academicYearDtos);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting academic years for school {SchoolId}", query.SchoolId);

            return Result<List<AcademicYearDto>>.Failed(
                Error.Failure(
                    "SchoolAcademicYearsQuery.Failed",
                    $"An error occurred while retrieving the school academic years: {ex.Message}"
                )
            );
        }
    }

    private static AcademicYearDto MapToAcademicYearDto(AcademicYear academicYear)
    {
        return new AcademicYearDto(
            Id: academicYear.Id,
            Name: academicYear.Name,
            Code: academicYear.Code,
            StartDate: academicYear.StartDate,
            EndDate: academicYear.EndDate,
            IsCurrent: academicYear.IsCurrent,
            SchoolId: academicYear.SchoolId,
            CreatedOn: academicYear.CreatedOn,
            ModifiedOn: academicYear.ModifiedOn
        );
    }
}